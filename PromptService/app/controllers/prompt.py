import logging
from typing import Annotated
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

logger = logging.getLogger(__name__)

class PromptController(Controller):
    tags=["Prompt"]

    @post("/prompt", response_model=CreatePromptResponse)
    def create(self, data: CreatePromptRequest, service: PromptService  = Depends(get_prompt_service)) -> CreatePromptResponse:
        """
        Creates a prompt task and generates the structure.json file necessary
        for generation of audio and photo.
        """
        response = None

        try:
            result = service.pregenerate_task(data)
            return result
        except Exception:
            logger.exception("Error in service")
            pass
        finally:
            raise HTTPException(status_code=500, detail="Something went terribly wrong")

    @put("/prompt", response_model=EditPromptResponse)
    def edit(self, data: EditPromptRequest, service: PromptService  = Depends(get_prompt_service)) -> EditPromptResponse:
        """
        Allows to modify the generated structure.json to add
        audio tags for Yandex Speechkit intonation.
        """
        response = None

        try:
            result = service.edit_task(data)
            return result
        except Exception:
            logger.exception("Error in service")
            pass
        finally:
            raise HTTPException(status_code=500, detail="Something went terribly wrong")

    @get("/prompt", response_model=GetPromptRequest)
    def get(self, data: GetPromptRequest, service: PromptService  = Depends(get_prompt_service)) -> GetPromptResponse:
        """
        Allows to get the existing prompt structure via its task id.
        """
        response = None

        try:
            result = service.get_task(data)
            return result
        except Exception:
            logger.exception("Error in service")
            pass
        finally:
            raise HTTPException(status_code=500, detail="Something went terribly wrong")
