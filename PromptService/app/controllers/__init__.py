"""
Initializes all controllers.
"""
import logging
from fastapi import FastAPI
from app.services.prompt import PromptService

# This is very cringe but we don't have time for a better implementation
prompt_service: PromptService
def get_prompt_service() -> PromptService:

    global prompt_service

    if not prompt_service is None:
        return prompt_service

    raise SystemError("Prompt Service is not ready")

def add_controllers(app: FastAPI):
    """Add all controllers to the app."""

    @app.get("/healthcheck", tags=["System"])
    def healthcheck():
        """
        Used by Docker for healthcheck capabilities.
        """
        return "healthy"

    logging.info("Adding controllers...")
