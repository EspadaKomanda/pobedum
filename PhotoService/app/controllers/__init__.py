"""
Initializes all controllers.
"""
import logging
from fastapi import FastAPI

def add_controllers(app: FastAPI):
    """Add all controllers to the app."""

    @app.get("/healthcheck", tags=["System"])
    def healthcheck():
        """
        Used by Docker for healthcheck capabilities.
        """
        return "healthy"

    logging.info("Adding controllers...")
