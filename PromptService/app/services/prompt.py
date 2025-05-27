"""
Prompt service for moderation and creation of instructions for generation of users' stories.
"""
import json
import logging
import tempfile
import os
from typing import Dict, Any

from app.services.kafka import ThreadedKafkaConsumer, KafkaProducerClient
from app.services.openai import OpenAIService, APIException
from app.services.s3 import S3Service

from app.config import DEEPSEEK_API_KEY, GEN_MODE, PROMPT_MODERATION, PROMPT_GENERATION

logger = logging.getLogger(__name__)


class PromptService:
    """
    Service responsible for moderation of user queries and providing instructions for
    PhotoService and AudioService.
    """

    def __init__(self, kafka_bootstrap_servers: str):
        self.logger = logging.getLogger(self.__class__.__name__)
        self.openai_service = OpenAIService(api_key=DEEPSEEK_API_KEY, mode="deepseek")
        
        self.consumer = ThreadedKafkaConsumer(
            topics=['generation_requests'], # TODO: topic to listen to pipline gen requests
            group_id='prompt-service',
            bootstrap_servers=kafka_bootstrap_servers,
            message_callback=self.process_message
        )
        
        self.producer = KafkaProducerClient(bootstrap_servers=kafka_bootstrap_servers)
        logger.info("Initiated the PromptService")

    def _moderate_prompt(self, prompt: str) -> bool:
        """
        Moderates the prompt using DeepSeek's API.
        Returns True if the prompt is appropriate, False otherwise.
        """
        if GEN_MODE == "plug":
            return True

        # moderation_instruction = (
        #     "Analyze the following prompt for inappropriate content (violence, hate speech, explicit material). "
        #     "Respond with a JSON object containing one boolean field 'ok'. Example: {'ok': true}.\n\n"
        #     f"Prompt: {prompt}"
        # )
        
        moderation_instruction = PROMPT_MODERATION.replace("%s", prompt)
        self.logger.debug("Using the following generation instruction: %s", moderation_instruction)

        try:
            response = self.openai_service.chat_completion(
                prompt=moderation_instruction,
                model="deepseek-chat",
                temperature=0.0,
                max_tokens=50
            )
            result = json.loads(response.replace("```json", "").replace("```", ""))
            return result.get('ok', False)
        except json.JSONDecodeError:
            self.logger.error("Moderation response is not valid JSON")
            return False
        except APIException as e:
            self.logger.error("Moderation API error: %s", e)
            return False

    def _generate_and_save_prompts(self, pipeline_guid: str, source_prompt: str) -> str:
        """
        Generates structured data (paragraphs + image prompts) and saves to S3.
        Returns the generated JSON string.
        """
        # generation_instruction = (
        #     "Split the story into paragraphs. For each, provide 'text' for audio and 'photo_prompt' for image generation. "
        #     "Also provide the voice (male is kirill and female is dasha) which should be used to voice the paragraph in field 'voice'."
        #         "Respond with a JSON array of objects. Example: [{'text': '...', 'photo_prompt': '...', 'voice': 'kirill'}]\n\n"
        #     f"Story: {source_prompt}"
        # )
        
        generation_instruction = PROMPT_GENERATION.replace("%s", source_prompt)
        self.logger.debug("Using the following generation instruction: %s", generation_instruction)
        try:
            
            response = ""
            if GEN_MODE == "plug":

                response = [
                    '[{"text": "Hello people1", "photo_prompt": "photo description", "voice": "john"},'
                    '{"text": "Hello people2", "photo_prompt": "photo description", "voice": "john"},'
                    '{"text": "Hello people3", "photo_prompt": "photo description", "voice": "john"},'
                    '{"text": "Hello people4", "photo_prompt": "photo description", "voice": "john"}]'
                ][0]

            else:

                response = self.openai_service.chat_completion(
                    prompt=generation_instruction,
                    model="deepseek-chat",
                    temperature=0.7,
                    max_tokens=1000
                )

            response = response.replace("```json", "").replace("```", "")
            paragraphs = json.loads(response)
            
            # Validate structure
            if not isinstance(paragraphs, list):
                raise ValueError("Expected a JSON array")
            for item in paragraphs:
                if 'text' not in item or 'photo_prompt' not in item or 'voice' not in item:
                    raise ValueError("Missing required fields in JSON item")
            
            # Save JSON to temporary file and upload to S3
            json_str = json.dumps(paragraphs, indent=2)
            with tempfile.NamedTemporaryFile(mode='w', delete=False) as f:
                f.write(json_str)
                temp_path = f.name
            
            S3Service.upload(
                bucket_name=pipeline_guid,
                destination="structure.json",
                source=temp_path,
                create_bucket_if_not_exists=True
            )
            os.unlink(temp_path)  # Cleanup temp file
            
            self.logger.info("Generated prompts saved to bucket '%s'", pipeline_guid)
            return json_str
            
        except (json.JSONDecodeError, ValueError) as e:
            self.logger.error("Prompt generation error: %s", e)
            raise
        except Exception as e:
            self.logger.error("Unexpected error: %s", e)
            raise

    def _request_start_audio(self, pipeline_guid: str, video_guid: str):
        """Notifies AudioService to start processing via Kafka."""
        message = json.dumps({
            "Action": "StartAudioGeneration", # TODO: make sure action is the same in audio service
            "TaskId": pipeline_guid,
            "VideoId": video_guid
        })
        self.producer.send_message("audio_requests", message)

    def _request_start_photo(self, pipeline_guid: str, video_guid: str):
        """Notifies PhotoService to start processing via Kafka."""
        message = json.dumps({
            "Action": "StartPhotoGeneration", # TODO: make sure action is the same in photo service
            "TaskId": pipeline_guid,
            "VideoId": video_guid
        })
        self.producer.send_message("photo_requests", message)

    def process_message(self, message: Dict[str, Any]):
        """Processes incoming Kafka messages from the pipeline."""
        try:
            pipeline_guid = message["PipelineId"]
            video_guid = message["VideoId"]
            user_prompt = message["Text"]
            
            self.producer.send_message(
                "status_update_requests",
                json.dumps({
                    "TaskId": pipeline_guid,
                    "Status": 1 # Analyze Letter
                })
            )

            # Step 1: Moderate prompt
            if not self._moderate_prompt(user_prompt):
                self.logger.warning("Prompt rejected for pipeline %s", pipeline_guid)
                self.producer.send_message(
                    "status_update_requests",
                    json.dumps({
                        "TaskId": pipeline_guid,
                        "Status": 10 # Cancelled (rejected) # XXX: use a better code
                    })
                )
                return
            
            # Step 2: Generate and save prompts
            self._generate_and_save_prompts(pipeline_guid, user_prompt)
            
            # Step 3: Trigger downstream services
            self._request_start_audio(pipeline_guid, video_guid)
            self._request_start_photo(pipeline_guid, video_guid)
            
            # Notify pipeline of success
            self.producer.send_message(
                "status_update_requests",
                json.dumps({
                    "TaskId": pipeline_guid,
                    "Status": 8 # Success
                })
            )
            
        except KeyError as e:
            self.logger.error("Invalid message format: missing %s", e)
            self.producer.send_message(
                "status_update_requests",
                json.dumps({
                    "TaskId": pipeline_guid,
                    "Status": 11 # Error
                })
            )
            return
        except Exception as e:
            self.logger.error("Failed to process message: %s", e)
            self.producer.send_message(
                "status_update_requests",
                json.dumps({
                    "TaskId": pipeline_guid,
                    "Status": 11 # Error
                })
            )
            raise

    def start(self):
        """Starts the Kafka consumer to begin processing messages."""
        self.consumer.start()
        
    def shutdown(self):
        """Gracefully shuts down the service."""
        self.consumer.stop()
        self.producer.close()
