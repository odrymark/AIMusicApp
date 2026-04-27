from pydantic import BaseModel, field_validator

class SongInputModel(BaseModel):
    lyrics: str
    bpm: int

    @field_validator("lyrics")
    @classmethod
    def validate_lyrics(cls, v: str) -> str:
        if not v.strip():
            raise ValueError("Lyrics cannot be empty.")
        return v

    @field_validator("bpm")
    @classmethod
    def validate_bpm(cls, v: int) -> int:
        if v <= 0:
            raise ValueError("BPM must be a positive number.")
        if v > 300:
            raise ValueError("BPM value is unrealistically high.")
        return v