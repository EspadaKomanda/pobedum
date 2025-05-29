"""
Initializes all controllers.
"""
import logging
from fastapi import FastAPI
from .prompt import PromptController

def add_controllers(app: FastAPI):
    """Add all controllers to the app."""

    @app.get("/healthcheck", tags=["System"])
    def healthcheck():
        """
        Used by Docker for healthcheck capabilities.
        """
        return "healthy"

    logging.info("Adding controllers...")
    app.include_router(PromptController.create_router())
