"""
This module serves as the entry point for the backend application.
"""
import logging
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
from app.utils.filters.fastapi_healthcheck import FastAPIHealthCheckFilter
from app import config
from app import models
from app import controllers

models.create_database()

app = FastAPI()
app.title = "Espada - Backend API"
app.summary = "Backend API for 'Kod Pobedi'"
app.contact = {"name": "Github", "url": "https://github.com/EspadaKomanda/pobedum"}

def main():
    """
    Entrypoint function
    """

    # Logging setup
    logging.basicConfig(
        level=logging.DEBUG if config.ENVIRONMENT_TYPE == "development" else logging.INFO,
        format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
        handlers=[
            logging.StreamHandler()  # Log to console
        ]
    )
    logging.getLogger("uvicorn.access").addFilter(FastAPIHealthCheckFilter())

    app.add_middleware(
        CORSMiddleware,
        allow_origins=[
            "127.0.0.1:8000"
        ],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    controllers.add_controllers(app)

    uvicorn.run(app, host="0.0.0.0", port=8000, log_config=None)

if __name__ == "__main__":
    main()
