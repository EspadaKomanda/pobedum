from app.services.prompt import PromptService

prompt_service: PromptService
def get_prompt_service() -> PromptService:

    global prompt_service

    if not prompt_service is None:
        return prompt_service

    raise SystemError("Prompt Service is not ready")
