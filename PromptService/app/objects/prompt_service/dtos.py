from pydantic import BaseModel, Json

class PromptParagraph(BaseModel, str_strip_whitespace=True):
    text: str
    photo_prompt: str
    voice: str
