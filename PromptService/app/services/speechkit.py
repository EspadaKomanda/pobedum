"""
Yandex SpeechKit service with S3 integration using official SDK.
"""
import logging
import tempfile
from speechkit import SpeechSynthesis
from fastapi import HTTPException
from app.services.s3 import S3Service

class YandexSpeechKitS3:
    """
    Yandex SpeechKit service with S3 integration using official SDK.

    Attributes:
        folder_id (str): Yandex Cloud folder ID
        iam_token (str): Yandex Cloud IAM token
        logger (logging.Logger): Configured logger instance
        synth (SpeechSynthesis): SpeechKit synthesis client
    """

    def __init__(self, folder_id: str, iam_token: str):
        """
        Initialize SpeechKit client with credentials.

        Args:
            folder_id (str): Yandex Cloud folder ID
            iam_token (str): Yandex Cloud IAM token
        """
        self.folder_id = folder_id
        self.iam_token = iam_token
        self.logger = logging.getLogger(self.__class__.__name__)
        self.logger.addHandler(logging.NullHandler())
 
        # Initialize SpeechKit client
        self.synth = SpeechSynthesis(iam_token=iam_token)

    def synthesize_to_s3(
        self,
        text: str,
        voice: str,
        bucket_name: str,
        s3_object_key: str,
        lang: str = "ru-RU",
        audio_format: str = "oggopus",
        speed: float = 1.0,
        emotion: str = "neutral"
    ) -> None:
        """
        Synthesize speech and upload directly to S3 storage.

        Args:
            text (str): Text to convert to speech
            voice (str): Voice type (e.g., 'alena', 'filipp', 'john')
            bucket_name (str): Target S3 bucket name
            s3_object_key (str): Destination object key in S3
            lang (str, optional): Language code. Defaults to 'ru-RU'
            audio_format (str, optional): Audio format. Options: 'lpcm', 'oggopus', 'mp3'
            speed (float, optional): Speech speed ratio. Defaults to 1.0
            emotion (str, optional): Emotional tone. Options: 'neutral', 'good', 'evil'

        Raises:
            ValueError: For invalid input parameters
            RuntimeError: For SpeechKit synthesis failures
            HTTPException: For S3 upload failures
        """
        self.logger.info("Initiating speech synthesis for %s characters", len(text))
        self.logger.debug("Voice: %s, Lang: %s, Format: %s", voice, lang, audio_format)

        # Validate input parameters
        if not text.strip():
            error_msg = "Input text cannot be empty"
            self.logger.error(error_msg)
            raise ValueError(error_msg)

        try:
            # Create temporary file for audio storage
            with tempfile.NamedTemporaryFile(suffix=f".{audio_format}", delete=True) as temp_file:
                # Perform speech synthesis
                self.synth.synthesize_to_file(
                    text=text,
                    voice=voice,
                    file_path=temp_file.name,
                    format=audio_format,
                    folder_id=self.folder_id,
                    lang=lang,
                    speed=speed,
                    emotion=emotion
                )

                self.logger.debug("Temporary audio file created: %s", temp_file.name)

                # Upload to S3 using provided service
                S3Service.upload(
                    bucket_name=bucket_name,
                    destination=s3_object_key,
                    source=temp_file.name
                )

            self.logger.info("Successfully uploaded to s3://%s/%s", bucket_name, s3_object_key)

        except Exception as e:
            error_msg = f"Synthesis/S3 upload failed: {str(e)}"
            self.logger.error(error_msg)

            # Convert library exceptions to appropriate error types
            if isinstance(e, HTTPException):
                raise
            raise RuntimeError(error_msg) from e
