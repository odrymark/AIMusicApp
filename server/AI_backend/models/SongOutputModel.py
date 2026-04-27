from pydantic import BaseModel
from typing import Literal

class SongOutputModel(BaseModel):
    mood: Literal["happy", "sad", "energetic", "calm", "angry", "romantic", "melancholic", "anxious", "unknown"]