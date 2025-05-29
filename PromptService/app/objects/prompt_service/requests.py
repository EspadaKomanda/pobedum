from typing import List, Dict
from pydantic import BaseModel, Json
from .dtos import PromptParagraph

class CreatePromptRequest(BaseModel, str_strip_whitespace=True):

    text: str
    color_scheme: str = "color"
    resolution: str = "1024x1024"
    model: str = "remote"
    frame_rate: int = 5

class EditPromptRequest(BaseModel, str_strip_whitespace=True):

    task_id: str
    content: List[Dict]
    pass

class GetPromptRequest(BaseModel, str_strip_whitespace=True):

    task_id: str
