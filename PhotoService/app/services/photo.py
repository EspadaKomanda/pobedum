"""
Service for generating photos with ChatGPT DALL·E from photo prompts.
"""
import json
import logging
import tempfile
import os
import requests
from typing import Dict, Any

from app.services.openai import OpenAIService, APIException
from app.services.s3 import S3Service
from app.services.kafka import ThreadedKafkaConsumer, KafkaProducerClient
from app.config import OPENAI_API_KEY, GEN_MODE

logger = logging.getLogger(__name__)

class PhotoService:
    """
    Service for generating images from text prompts and managing their storage in S3.
    """
    
    def __init__(self, kafka_bootstrap_servers: str):
        self.logger = logging.getLogger(self.__class__.__name__)
        self.openai_service = OpenAIService(api_key=OPENAI_API_KEY, mode="openai")
        
        # Initialize Kafka consumer for photo requests
        self.consumer = ThreadedKafkaConsumer(
            bootstrap_servers=kafka_bootstrap_servers,
            group_id='photo-service',
            topics=['photo_requests'],
            message_callback=self.process_message
        )
        
        # Initialize Kafka producer for notifications
        self.producer = KafkaProducerClient(
            bootstrap_servers=kafka_bootstrap_servers,
            value_serializer=lambda v: json.dumps(v).encode('utf-8')
        )

    def _download_image(self, url: str) -> str:
        """Downloads image from URL to temporary file and returns path."""
        try:
            response = requests.get(url, timeout=10)
            response.raise_for_status()
            
            with tempfile.NamedTemporaryFile(delete=False, suffix=".jpg") as f:
                f.write(response.content)
                return f.name
        except Exception as e:
            self.logger.error("Failed to download image: %s", e)
            raise

    def _generate_photos(self, pipeline_guid: str, video_guid: str):
        """Generates and stores images for all prompts in structure.json."""
        try:
            # Download structure.json from S3
            with tempfile.NamedTemporaryFile(delete=False) as temp_file:
                S3Service.download(
                    bucket_name=pipeline_guid,
                    source="structure.json",
                    destination=temp_file.name
                )
                with open(temp_file.name, 'r', encoding='utf-8') as f:
                    structure = json.load(f)
                os.unlink(temp_file.name)

            photo_files = []
            for idx, paragraph in enumerate(structure, start=1):
                prompt = paragraph['photo_prompt']
                
                image_urls = []

                if GEN_MODE == 'plug':

                    image_urls = ["http://cloud.weirdcat.su/s/plug_photo/download/plug.jpg"]

                else:

                    # Generate single HD quality image
                    image_urls = self.openai_service.generate_image(
                        prompt=prompt,
                        n=1,
                        size="1024x1024",
                        quality="hd"
                    )
                
                # Download and store image
                temp_image_path = self._download_image(image_urls[0])
                s3_key = f"photos/photo{idx}.jpg"
                
                S3Service.upload(
                    bucket_name=pipeline_guid,
                    destination=s3_key,
                    source=temp_image_path,
                    create_bucket_if_not_exists=True
                )
                os.unlink(temp_image_path)
                photo_files.append(s3_key)

            # Create and upload photo manifest
            manifest = {"photo_files": photo_files}
            with tempfile.NamedTemporaryFile(mode='w', delete=False) as f:
                json.dump(manifest, f)
                temp_path = f.name
                
            S3Service.upload(
                bucket_name=pipeline_guid,
                destination="photos.json",
                source=temp_path,
                create_bucket_if_not_exists=True
            )
            os.unlink(temp_path)

        except Exception as e:
            self.logger.error("Photo generation failed: %s", e)
            raise

    def process_message(self, message: Dict[str, Any], _):
        """Processes incoming Kafka messages to generate images."""
        pipeline_guid = None
        try:
            if message.get('action') != 'StartPhotoGeneration':
                return

            pipeline_guid = message['TaskId']
            video_guid = message['VideoId']
            self.logger.info("Starting image generation for pipeline %s", pipeline_guid)

            self._generate_photos(pipeline_guid, video_guid)

            # Notify MergeService
            self.producer.send_message(
                topic="merge_requests",
                value={
                    "Status": "photos_completed",
                    "TaskId": pipeline_guid,
                    "VideoId": video_guid
                }
            )

            # # Notify PipelineService
            # self.producer.send_message(
            #     topic="statusUpdates",
            #     value={
            #         "TaskId": pipeline_guid,
            #         "status": "photos_finished"
            #     }
            # )

        except Exception as e:
            self.logger.error("Photo processing failed: %s", e)
            if pipeline_guid:
                self.producer.send_message(
                    topic="pipeline_responses",
                    value={
                        "pipeline_guid": pipeline_guid,
                        "status": "error",
                        "reason": str(e)
                    }
                )

    def start(self):
        """Starts the Kafka consumer thread."""
        self.consumer.start()
        self.logger.info("PhotoService started")

    def shutdown(self):
        """Gracefully shuts down the service."""
        self.consumer.stop()
        self.producer.flush()
        self.logger.info("PhotoService shut down")
