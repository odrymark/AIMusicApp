from pydantic import BaseModel, field_validator
from typing import List

class ListenedSong(BaseModel):
    mood: str
    title: str
    artist: str

class AvailableSong(BaseModel):
    id: str
    mood: str
    title: str
    artist: str

class RecommendInputModel(BaseModel):
    listened_moods: List[ListenedSong]
    available_songs: List[AvailableSong]

    @field_validator("listened_moods")
    @classmethod
    def validate_listened_moods(cls, v: List[ListenedSong]) -> List[ListenedSong]:
        if not v:
            raise ValueError("Listened moods cannot be empty.")
        return v

    @field_validator("available_songs")
    @classmethod
    def validate_available_songs(cls, v: List[AvailableSong]) -> List[AvailableSong]:
        if not v:
            raise ValueError("Available songs cannot be empty.")
        return v