"""
S3 service for managing files.
"""
import logging
from fastapi import HTTPException, status
from minio import Minio
from minio.error import S3Error

from app.config import (
    MINIO_HOST,
    MINIO_PORT,
    MINIO_ROOT_USER,
    MINIO_ROOT_PASSWORD
)

logger = logging.getLogger(__name__)

client = Minio(
    endpoint=f"{MINIO_HOST}:{MINIO_PORT}",
    access_key=MINIO_ROOT_USER,
    secret_key=MINIO_ROOT_PASSWORD,
    secure=False
)

class S3Service:
    """
    S3 service for managing files.
    """
    @classmethod
    def upload(
        cls,
        bucket_name: str,
        destination: str,
        source: str,
        create_bucket_if_not_exists: bool = True
    ):
        """
        Uploads a file to a bucket.
        """
        try:
            if create_bucket_if_not_exists:
                if not cls.bucket_exists(bucket_name):
                    logger.info("Bucket %s does not exist, creating...", bucket_name)
                    cls.create_bucket(bucket_name)
            else:
                if not cls.bucket_exists(bucket_name):
                    raise HTTPException(
                        status_code=status.HTTP_404_NOT_FOUND,
                        detail=f"Bucket {bucket_name} does not exist."
                    )

            client.fput_object(bucket_name, destination, source)
            logger.info("File %s uploaded to bucket %s", source, bucket_name)
        except S3Error as e:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=str(e)
            ) from e

    @classmethod
    def download(cls, bucket_name: str, source: str, destination: str):
        """
        Downloads a file from a bucket.
        """
        try:
            client.fget_object(bucket_name, source, destination)
            logger.info("File %s downloaded from bucket %s", source, bucket_name)
            return True
        except S3Error as e:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=str(e)
            ) from e

    @classmethod
    def create_bucket(cls, bucket_name: str):
        """
        Creates a new bucket.
        """
        client.make_bucket(bucket_name)
        logger.info("Bucket %s created successfully.", bucket_name)

    @classmethod
    def bucket_exists(cls, bucket_name: str) -> bool:
        """
        Checks if a bucket exists.
        """
        exists = client.bucket_exists(bucket_name)
        logger.info("Bucket %s exists: %s", bucket_name, exists)
        return exists
