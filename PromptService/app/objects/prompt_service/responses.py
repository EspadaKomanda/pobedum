from pydantic import BaseModel, Json

class CreatePromptResponse(BaseModel):
    
    task_id: str

class EditPromptResponse(BaseModel):
    
    task_id: str

class GetPromptResponse(BaseModel):
    
    task_id: str
    content: Json
