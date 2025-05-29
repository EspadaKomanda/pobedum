from typing import List, Dict
from pydantic import BaseModel, Json

class CreatePromptResponse(BaseModel):
    
    task_id: str

class EditPromptResponse(BaseModel):
    
    task_id: str
    content: List[Dict]

class GetPromptResponse(BaseModel):
    
    task_id: str
    content: Json
