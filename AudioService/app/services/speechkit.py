"""
A service class for integrating Yandex SpeechKit with S3 storage for audio files.
"""
import logging
import requests
from tempfile import NamedTemporaryFile
from typing import List

from speechkit import model_repository, configure_credentials, creds
from speechkit.stt import AudioProcessingType
# from pydub import AudioSegment

from app.services.s3 import S3Service
from app.config import YANDEX_SPEECHKIT_API_KEY, GEN_MODE

logger = logging.getLogger(__name__)

# Configure Yandex SpeechKit credentials using the API key from app.config
if not YANDEX_SPEECHKIT_API_KEY:
    raise ValueError("YANDEX_SPEECHKIT_API_KEY is not set in app.config")

configure_credentials(
    yandex_credentials=creds.YandexCredentials(api_key=YANDEX_SPEECHKIT_API_KEY)
)

class YandexSpeechKitService:
    """
    A service class for integrating Yandex SpeechKit with S3 storage for audio files.

    This class provides methods to synthesize text to audio stored in S3,
    leveraging Yandex SpeechKit's capabilities for speech-to-text and text-to-speech conversions.
    """

    def synthesize_to_s3(
        self, text: str, s3_bucket: str, s3_key: str, voice: str = "john"
    ) -> str:
        """
        Synthesizes text to speech and uploads the resulting audio to an S3 bucket.

        Args:
            text (str): The text to synthesize into speech.
            s3_bucket (str): The name of the S3 bucket to upload the audio file.
            s3_key (str): The key for the audio file in the S3 bucket.
            voice (str, optional): The voice model to use for synthesis. Defaults to 'john'.

        Returns:
            str: The S3 path (s3://bucket/key) of the uploaded audio file.

        Raises:
            HTTPException: If there is an error synthesizing the audio or uploading to S3.
        """
        try:

            if GEN_MODE == 'plug':

                audio_url = "http://cloud.weirdcat.su/s/oiojoijoij/download/maks_plug_sound.wav"
                response = requests.get(audio_url)
                response.raise_for_status()  # Raise an error for bad responses

                with NamedTemporaryFile(suffix=".mp3", delete=False) as temp_audio:
                    temp_audio.write(response.content)
                    temp_audio_path = temp_audio.name
                    logger.info("Downloaded audio to temporary file: %s", temp_audio_path)

                # Upload the downloaded audio to S3
                S3Service.upload(s3_bucket, s3_key, temp_audio_path, True)
                logger.info("Uploaded audio to s3://%s/%s", s3_bucket, s3_key)

                return f"s3://{s3_bucket}/{s3_key}"

            else:

                model = model_repository.synthesis_model()
                model.voice = voice

                audio_segment = model.synthesize(text, raw_format=False)
                logger.info("Synthesized audio for text: %s", text)

                with NamedTemporaryFile(suffix=".wav") as temp_audio:
                    audio_segment.export(temp_audio.name, format="wav")
                    logger.info("Exported audio to temporary file: %s", temp_audio.name)

                    S3Service.upload(s3_bucket, s3_key, temp_audio.name, True)
                    logger.info(
                        "Uploaded synthesized audio to s3://%s/%s", s3_bucket, s3_key
                    )

                    return f"s3://{s3_bucket}/{s3_key}"

        except Exception as e:
            logger.error(
                "Failed to synthesize text and upload to S3: %s", e, exc_info=True
            )
            raise
