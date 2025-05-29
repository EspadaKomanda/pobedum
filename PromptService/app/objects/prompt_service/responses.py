from pydantic import BaseModel, Json

class CreatePromptResponse(BaseModel, str_strip_whitespace=True):
    
    task_id: str
    content: Json

class EditPromptResponse(BaseModel, str_strip_whitespace=True):
    
    task_id: str

class GetPromptResponse(BaseModel, str_strip_whitespace=True):
    
    task_id: str
    content: Json
