"""
Project-specific validation functions.
Split into a different module since not all validation functions
may be compatible with other projects and will need adjustments.
"""
import re
from .standard import v_digits, v_length

def v_name(value: str):
    """
    Validates that a string is a valid name.
    """
    name_regex = r'.{1,100}$'

    return re.match(name_regex, value)

def v_username(value: str):
    """
    Validates that a username contains only Latin characters, numbers, and underscores.
    The username must be between 3 and 18 characters long.
    """
    username_regex = r'^[a-zA-Z0-9_]{3,18}$'

    return re.match(username_regex, value)

def v_password(value: str):
    """
    Validates that a string is a valid password.
    Password must be 8-50 characters long.
    Password must contain at least one lowercase and one uppercase letter.
    Password must contain at least one digit.
    Password must contain at least one special character.
    Only Latin characters are allowed.
    """
    password_regex = r'^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}[\]:;"<>?/\|`~,.])(?=.{8,50}$).*$'  # pylint: disable=line-too-long

    return re.match(password_regex, value)

def v_registration_code(value: str):
    """
    Validates that a string is a valid registration code.
    """
    return v_length(value, max_length=6) and v_digits(value)
