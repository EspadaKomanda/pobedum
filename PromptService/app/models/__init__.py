"""
Initialization of database
"""
import logging
from app.config import POSTGRES_DB, ENVIRONMENT_TYPE
from .base import db

logger = logging.getLogger(__name__)

tables: list = [
    ]

logger.debug("Connecting to databse...")
db.connect()

def create_database():
    """
    Automatically initializes all tables.
    """
    logger.debug("Initializing tables...")
    db.create_tables(
        tables,
        safe=True
    )

    logger.debug("All tables have been initialized successfully.")

def wipe_database(database_name: str):
    """
    Removes the contents of the database being used. the database_name parameter is
    compared to the name of the open database to avoid deletion of the wrong database.
    Running wipe_database in an environment other than "development" is also forbidden.
    """
    logger.debug("Wiping database")

    if POSTGRES_DB != database_name:
        raise RuntimeError(
            f"Attempting to wipe a different database. "
            f"Currently running with {POSTGRES_DB}"
        )
    if ENVIRONMENT_TYPE != "development":
        raise RuntimeError("Attempting to wipe database in non-development environment.")

    db.drop_tables(tables, safe=True)

    logger.debug("All tables have been dropped successfully.")
