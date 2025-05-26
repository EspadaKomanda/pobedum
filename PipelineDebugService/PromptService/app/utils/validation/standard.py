"""
Standard validation features that do not depend
on project's specifics. Recommended to use with
Pydantic and Peewee.
"""
import re
from typing import Optional

def validate_field(cls, field: str, func, *args, **kwargs):
    """
    Function for validating fields in peewee models.
    field - name of the field
    func - validation function
    *args, **kwargs - additional arguments
    """
    value = getattr(cls, field)
    result = value is None or func(value, *args, **kwargs)
    if not result:
        raise ValueError(f"Validation failed for {func.__name__} on field {field}.")

def v_regex(value: str, expr: str):
    """
    Validates field using the given regex. Ignores null values.
    
    Returns True if validation passes, False otherwise.
    """
    # Validate
    return re.match(expr, value)

def v_email(value: str):
    """
    Validates that a string is a valid email address.
    """
    email_regex = r'^[^@]+@[^@]+\.[a-zA-Zа-яА-Я]{2,}$'

    return re.match(email_regex, value)

def v_url(value: str):
    """
    Validates that a string is a valid URL.
    """
    url_regex = r"""^(?!.*\.\.)(?:http(s)?:\/\/)?[\wа-яА-Я.-]+(?:\.[\wа-яА-Я.-]+)+[\wа-яА-я\-._~:/?#[\]@!\$&'$$\*\+,;=.]+$"""  # pylint: disable=line-too-long

    return re.match(url_regex, value)

def v_guid(value: str):
    """
    Validates that a string is a valid GUID.
    """
    guid_regex = r'^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$'

    return re.match(guid_regex, value)

def v_length(
    value: str,
    precise_length: Optional[int] = None,
    min_length: Optional[int] = None,
    max_length: Optional[int] = None):
    """
    Validates that a string is within the given length range.
    precise_length takes precedence over min_length and max_length.
    """
    if precise_length is None and min_length is None and max_length is None:
        raise RuntimeError(
            "At least one of min_length, max_length or precise_length must be specified."
            )

    expr = r'^.*$'  # pylint: disable=f-string-without-interpolation

    if precise_length is not None:
        expr = rf'^.{{{precise_length}}}$'
    elif min_length is not None and max_length is not None:
        expr = rf'^.{{{min_length},{max_length}}}$'
    elif min_length is not None:
        expr = rf'^.{{{min_length},}}$'
    elif max_length is not None:
        expr = rf'^.{{,{max_length}}}$'

    return re.match(expr, value)

def v_digits(value: str):
    """
    Validates that a string only contains digits.
    """
    digits_regex = r'^\d+$'

    return re.match(digits_regex, value)
