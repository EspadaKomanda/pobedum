from pydantic import BaseModel, Json

class CreatePromptRequest(BaseModel, str_strip_whitespace=True):

    text: str
    color_scheme: str = "color"
    resolution: str = "1024x1024"
    model: str = "remote"
    frame_rate: int = 5

class EditPromptRequest(BaseModel, str_strip_whitespace=True):

    task_id: str
    content: Json
    pass

class GetPromptRequest(BaseModel, str_strip_whitespace=True):

    task_id: str
