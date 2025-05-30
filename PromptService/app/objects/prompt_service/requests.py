from typing import List, Dict
from pydantic import BaseModel, Json, validator
from .dtos import PromptParagraph

class CreatePromptRequest(BaseModel, str_strip_whitespace=True):

    text: str
    color_scheme: str = "color"
    resolution: str = "1024x1024"
    model: str = "remote"
    frame_rate: int = 5

    @validator('resolution')
    def validate_resolution(cls, value):
        if value not in ['512x512', '1024x1024']:
            raise ValueError('Resolution must be either "512x512" or "1024x1024".')
        return value

class EditPromptRequest(BaseModel, str_strip_whitespace=True):

    task_id: str
    content: List[Dict]
    pass

class GetPromptRequest(BaseModel, str_strip_whitespace=True):

    task_id: str
