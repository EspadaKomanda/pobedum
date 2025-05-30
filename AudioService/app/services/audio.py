"""
AudioService generates audio from given paragraphs with Yandex Speechkit.
"""
import json
import logging
import tempfile
import os
from typing import Dict, Any

from app.services.kafka import ThreadedKafkaConsumer, KafkaProducerClient
from app.services.speechkit import YandexSpeechKitService
from app.services.s3 import S3Service

logger = logging.getLogger(__name__)

class AudioService:
    """
    Service for generating audio files from text paragraphs and managing their storage in S3.
    """
    
    def __init__(self, kafka_bootstrap_servers: str):
        self.logger = logging.getLogger(self.__class__.__name__)
        self.speechkit = YandexSpeechKitService()
        
        # Initialize Kafka consumer for audio requests
        self.consumer = ThreadedKafkaConsumer(
            bootstrap_servers=kafka_bootstrap_servers,
            group_id='audio-service',
            topics=['audio_requests'],
            message_callback=self.process_message
        )
        
        # Initialize Kafka producer for notifications
        self.producer = KafkaProducerClient(
            bootstrap_servers=kafka_bootstrap_servers,
            value_serializer=lambda v: json.dumps(v).encode('utf-8')
        )

    def _generate_and_save_audio(
        self,
        text: str,
        bucket_id: str,
        file_id: str,
        video_guid: str,
        voice: str,
        mood: str = "neutral"
    ) -> str:
        """
        Generates audio file from text and saves it to S3.
        Returns S3 path of the generated audio file.
        """
        try:
            s3_key = f"audio/{file_id}.wav"
            s3_path = self.speechkit.synthesize_to_s3(
                text=text,
                s3_bucket=bucket_id,
                s3_key=s3_key,
                voice=voice,
                mood=mood
            )
            self.logger.info("Generated audio for file %s in bucket %s", file_id, bucket_id)
            return s3_path
        except Exception as e:
            self.logger.error("Audio generation failed: %s", e)
            raise

    def process_message(self, message: Dict[str, Any]):
        """
        Processes incoming Kafka messages to generate audio files.
        """
        try:
            if not message.get('Action') == 'StartAudioGeneration':
                self.logger.error("Unknown Action '%s'", message.get('Action'))
                return

            pipeline_guid = message['TaskId']
            video_guid = message['VideoId']
            self.logger.info("Starting audio generation for pipeline %s", pipeline_guid)

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

            # Generate audio files
            audio_files = []
            for idx, paragraph in enumerate(structure, start=1):
                voice = paragraph.get('voice', 'john')
                mood = paragraph.get('mood', 'neutral')
                audio_path = self._generate_and_save_audio(
                    text=paragraph['text'],
                    bucket_id=pipeline_guid,
                    file_id=f"audio{idx}",
                    video_guid=video_guid,
                    voice=voice,
                    mood=mood
                )
                audio_files.append(os.path.basename(audio_path))

            # Create and upload audio manifest
            manifest = {"audio_files": audio_files}
            with tempfile.NamedTemporaryFile(mode='w', delete=False) as f:
                json.dump(manifest, f)
                temp_path = f.name
                
            S3Service.upload(
                bucket_name=pipeline_guid,
                destination="audio.json",
                source=temp_path,
                create_bucket_if_not_exists=True
            )
            os.unlink(temp_path)

            # Notify MergeService
            self.producer.send_message(
                topic="merge_requests",
                value={
                    "Status": "audio_completed",
                    "TaskId": pipeline_guid,
                    "VideoId": video_guid
                }
            )

            # # Notify PipelineService
            # self.producer.send_message(
            #     topic="pipeline_responses",
            #     value={
            #         "pipeline_guid": pipeline_guid,
            #         "status": "audio_finished"
            #     }
            # )

        except Exception as e:
            self.logger.error("Audio processing failed: %s", e)
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
        self.logger.info("AudioService started")

    def shutdown(self):
        """Gracefully shuts down the service."""
        self.consumer.stop()
        self.producer.flush()
        self.logger.info("AudioService shut down")
