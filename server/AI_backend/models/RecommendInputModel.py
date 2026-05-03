from pydantic import BaseModel, field_validator
from typing import List

class RecommendInputModel(BaseModel):
    listened_moods: List[str]
    available_songs: List[dict]

    @field_validator("listened_moods")
    @classmethod
    def validate_listened_moods(cls, v: List[str]) -> List[str]:
        if not v:
            raise ValueError("Listened moods cannot be empty.")
        return v

    @field_validator("available_songs")
    @classmethod
    def validate_available_songs(cls, v: List[dict]) -> List[dict]:
        if not v:
            raise ValueError("Available songs cannot be empty.")
        return v