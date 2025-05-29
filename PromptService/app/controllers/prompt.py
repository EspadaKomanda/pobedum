import logging
from typing import Annotated, List
from fastapi_controllers import Controller, get, post, put
from fastapi import Depends, HTTPException, Query

from app.controllers.depends import get_prompt_service
from app.services.prompt import PromptService

from app.objects.prompt_service.requests import (
    CreatePromptRequest,
    EditPromptRequest,
    GetPromptRequest
)

from app.objects.prompt_service.responses import (
    CreatePromptResponse,
    EditPromptResponse,
    GetPromptResponse
)

from app.objects.prompt_service.dtos import PromptParagraph

from app.exceptions.prompt import PromptEditInvalidatedException

logger = logging.getLogger(__name__)

class PromptController(Controller):
    tags=["Prompt"]

    @post("/prompt", response_model=CreatePromptResponse)
    def create(self, data: CreatePromptRequest, service: PromptService  = Depends(get_prompt_service)) -> CreatePromptResponse:
        """
        Creates a prompt task and generates the structure.json file necessary
        for generation of audio and photo.
        """
        try:
            result = service.pregenerate_task(data)
            return result
        except Exception:
            logger.exception("Error in service")
            raise HTTPException(status_code=500, detail="Something went terribly wrong")

    @put("/prompt", response_model=EditPromptResponse)
    def edit(self, data: EditPromptRequest, service: PromptService  = Depends(get_prompt_service)) -> EditPromptResponse:
        """
        Allows to modify the generated structure.json to add
        audio tags for Yandex Speechkit intonation.
        """
        try:
            result = service.edit_task(data)
            return result
        except PromptEditInvalidatedException:
            raise HTTPException(status_code=400, detail="Forbidden prompt alterations")
        except Exception:
            logger.exception("Error in service")
            raise HTTPException(status_code=500, detail="Something went terribly wrong")

    @get("/prompt/{task_id}", response_model=GetPromptResponse)
    def get(self, task_id: str, service: PromptService  = Depends(get_prompt_service)) -> GetPromptResponse:
        """
        Allows to get the existing prompt structure via its task id.
        """
        try:
            result = service.get_task(task_id)
            return result
        except Exception:
            logger.exception("Error in service")
            raise HTTPException(status_code=500, detail="Something went terribly wrong")
