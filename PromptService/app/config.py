"""
Configuration file for the application.

Utilizes environment variables and makes them accessible
everywhere else.

Do not modify this file, configure variables in your
environment instead.
"""
from os import getenv
from dotenv import load_dotenv

load_dotenv(override=False)

# Environment
ENVIRONMENT_TYPE=getenv("ENVIRONMENT_TYPE") or "development"

# Minio
MINIO_ROOT_USER=getenv("MINIO_ROOT_USER") or "minio"
MINIO_ROOT_PASSWORD=getenv("MINIO_ROOT_PASSWORD") or "minio"

if (MINIO_ROOT_USER is None or
    MINIO_ROOT_PASSWORD is None):
    raise SystemError("Not all minio parameters have been configured.")

# PosgreSQL
POSTGRES_DB=getenv("POSTGRES_DB")
POSTGRES_USER=getenv("POSTGRES_USER")
POSTGRES_PASSWORD=getenv("POSTGRES_PASSWORD")

if (POSTGRES_DB is None or
    POSTGRES_USER is None or
    POSTGRES_PASSWORD is None):
    raise SystemError("Not all database parameters have been configured.")

# Redis
REDIS_PASSWORD=getenv("REDIS_PASSWORD")

if REDIS_PASSWORD is None:
    raise SystemError("Redis password have not been configured.")
