from pydantic import BaseModel, field_validator
from typing import List

class RecommendOutputModel(BaseModel):
    song_ids: List[str]

    @field_validator("song_ids")
    @classmethod
    def validate_song_ids(cls, v: List[str]) -> List[str]:
        if not v:
            raise ValueError("Recommendation returned no song IDs.")
        if len(v) > 5:
            raise ValueError(f"Too many recommendations returned: {len(v)}, max is 5.")
        return v