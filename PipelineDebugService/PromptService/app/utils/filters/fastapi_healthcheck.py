"""
Filter designed to remove healthcheck entries from logs
"""
import logging

class FastAPIHealthCheckFilter(logging.Filter):
    """
    Filter out healthcheck endpoints
    """
    def filter(self, record: logging.LogRecord) -> bool:
        if isinstance(record.args, tuple) and len(record.args) >= 3:
            return record.args[2] != "/healthcheck"
        return True
