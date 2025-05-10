"""Service for interaction with OpenAI's API for ChatGPT."""
import logging
from typing import List, Literal
from openai import OpenAI
from app.exceptions.openai import APIException

class OpenAIService:
    """
    A service class for interacting with OpenAI's API,
    supporting both chat completions and image generation.

    Attributes:
        client (OpenAI): An instance of the OpenAI client configured with the provided API key.
    """

    def __init__(self, api_key: str, mode: str):
        """
        Initializes the OpenAIService with the provided API key.

        Args:
            api_key (str): The OpenAI API key used for authentication.
        """
        self.logger = logging.getLogger(self.__class__.__name__)

        if mode == "deepseek":
            self.client = OpenAI(api_key=api_key, base_url="https://api.deepseek.com/v1")
        elif mode == "openai":
            self.client = OpenAI(api_key=api_key)
        self.logger.debug("OpenAI service initialized")


    def chat_completion(
        self,
        prompt: str,
        model: str = "gpt-3.5-turbo",
        temperature: float = 0.7,
        max_tokens: int = 300
    ) -> str:
        """
        Generates a text completion for a given prompt using the specified OpenAI model.

        Args:
            prompt (str): The input text/prompt to generate completion for.
            model (str, optional): The OpenAI model to use. Defaults to "gpt-3.5-turbo".
            temperature (float, optional): Controls randomness (0.0 to 2.0). 
                Lower = more deterministic. Defaults to 0.7.
            max_tokens (int, optional): Maximum number of tokens to generate. Defaults to 300.

        Returns:
            str: The generated text completion.

        Raises:
            APIError: For any API-related errors
        """
        response = self.client.chat.completions.create(
            model=model,
            messages=[{"role": "user", "content": prompt}],
            temperature=temperature,
            max_tokens=max_tokens,
            stream=False
        )

        if not response.choices or not response.choices[0].message.content:
            self.logger.error("No content received in the response")
            raise APIException("No content received in the response")

        response_text = response.choices[0].message.content

        self.logger.debug("Generated completion: %s", response_text)

        return response_text

    def generate_image(
        self,
        prompt: str,
        n: int = 1,
        size: Literal["256x256", "512x512", "1024x1024",
                      "1024x1792", "1792x1024"] = "1024x1024",
        quality: Literal["standard", "hd"] = "standard"
    ) -> List[str]:
        """
        Generates images based on a text prompt using DALL·E model.

        Args:
            prompt (str): The text prompt to generate images for
            n (int, optional): Number of images to generate (1-10). Defaults to 1
            size (str, optional): Image resolution. Defaults to "1024x1024"
            quality (str, optional): Image quality. Defaults to "standard"

        Returns:
            List[str]: A list of URLs for the generated images

        Raises:
            ValueError: For invalid parameters
            APIError: For any API-related errors
        """
        if n < 1 or n > 10:
            self.logger.error("Number of images must be between 1 and 10")
            raise ValueError("Number of images must be between 1 and 10")

        response = self.client.images.generate(
            model="dall-e-3",
            prompt=prompt,
            n=n,
            size=size,
            quality=quality
        )

        return [image.url for image in response.data if image.url]
