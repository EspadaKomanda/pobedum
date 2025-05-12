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
MINIO_HOST=getenv("MINIO_HOST") or "minio"
MINIO_PORT=getenv("MINIO_PORT") or "9000"
MINIO_ROOT_USER=getenv("MINIO_ROOT_USER") or "minio"
MINIO_ROOT_PASSWORD=getenv("MINIO_ROOT_PASSWORD") or "minio"

if (MINIO_ROOT_USER is None or
    MINIO_ROOT_PASSWORD is None):
    raise SystemError("Not all minio parameters have been configured.")

# PosgreSQL
POSTGRES_HOST=getenv("POSTGRES_HOST") or "postgres"
POSTGRES_PORT=getenv("POSTGRES_PORT") or "5432"
POSTGRES_DB=getenv("POSTGRES_DB")
POSTGRES_USER=getenv("POSTGRES_USER")
POSTGRES_PASSWORD=getenv("POSTGRES_PASSWORD")

if (POSTGRES_DB is None or
    POSTGRES_USER is None or
    POSTGRES_PASSWORD is None):
    raise SystemError("Not all database parameters have been configured.")

# Redis
REDIS_HOST=getenv("REDIS_HOST") or "redis"
REDIS_PORT=getenv("REDIS_PORT") or "6379"
REDIS_PASSWORD=getenv("REDIS_PASSWORD")

if REDIS_PASSWORD is None:
    raise SystemError("Redis password have not been configured.")

# Kafka
KAFKA_HOST=getenv("KAFKA_HOST") or "kafka"
KAFKA_PORT=getenv("KAFKA_PORT") or "9092"
KAFKA_BROKERS=getenv("KAFKA_BROKERS") or f"{KAFKA_HOST}:{KAFKA_PORT}"

# OpenAI credentials
OPENAI_API_KEY=getenv("OPENAI_API_KEY")

if OPENAI_API_KEY is None:
    raise SystemError("OpenAI API key have not been configured.")

# DeepSeek credentials
DEEPSEEK_API_KEY=getenv("DEEPSEEK_API_KEY")

if DEEPSEEK_API_KEY is None:
    raise SystemError("DeepSeek API key have not been configured.")

# Yandex Speechkit credentials
YANDEX_SPEECHKIT_API_KEY=getenv("YANDEX_SPEECHKIT_API_KEY")

if YANDEX_SPEECHKIT_API_KEY is None:
    raise SystemError("Yandex Speechkit API key have not been configured.")

ENV_PROXY=getenv("ENV_PROXY") or None
