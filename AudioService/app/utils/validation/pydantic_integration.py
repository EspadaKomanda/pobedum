"""
Validation annotations for Pydantic models
"""
from typing import Any
from pydantic_core import core_schema
from .special import (
    v_name,
    v_password,
    v_username,
    v_registration_code
)
from .standard import (
    v_guid
)

class Base(str):
    """
    Base model for Pydantic models
    """
    @classmethod
    def __get_pydantic_core_schema__(
            cls, _source_type: Any, _handler: Any
    ) -> core_schema.CoreSchema:
        return core_schema.json_or_python_schema(
            json_schema=core_schema.str_schema(),
            python_schema=core_schema.union_schema([
                core_schema.is_instance_schema(cls),
                core_schema.chain_schema([
                    core_schema.str_schema(),
                    core_schema.no_info_plain_validator_function(cls.validate),
                ])
            ]),
            serialization=core_schema.plain_serializer_function_ser_schema(
                lambda x: str(x)  # pylint: disable=unnecessary-lambda
            ),
        )

    @classmethod
    def validate(cls, value: Any):
        """
        Validates the model.
        """
        return value

class Name(Base):
    """
    Name validator
    """
    @classmethod
    def __get_validators__(cls):
        yield cls.validate

    @classmethod
    def validate(cls, value: Any):
        """
        Validates name.
        """
        if not v_name(value):
            raise ValueError("Invalid name. ")

        return value

class Password(Base):
    """
    Password validator
    """
    @classmethod
    def __get_validators__(cls):
        yield cls.validate

    @classmethod
    def validate(cls, value: Any):
        """
        Validates password.
        """
        if not v_password(value):
            raise ValueError("Insufficiently strong password.")

        return value

class Username(Base):
    """
    Username validator
    """
    @classmethod
    def __get_validators__(cls):
        yield cls.validate

    @classmethod
    def validate(cls, value: Any):
        """
        Validates username.
        """
        if not v_username(value):
            raise ValueError("Invalid username.")

        return value

class Guid(Base):
    """
    GUID validator
    """
    @classmethod
    def __get_validators__(cls):
        yield cls.validate

    @classmethod
    def validate(cls, value: Any):
        """
        Validates GUID.
        """
        if not v_guid(value):
            raise ValueError("Invalid GUID.")

        return value

class RegistrationCode(Base):
    """
    Registration code validator
    """
    @classmethod
    def __get_validators__(cls):
        yield cls.validate

    @classmethod
    def validate(cls, value: Any):
        """
        Validates registration code.
        """
        if not v_registration_code(value):
            raise ValueError("Invalid registration code.")

        return value
