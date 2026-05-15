import traceback
from typing import Literal, cast

import librosa
from fastapi import FastAPI, HTTPException, UploadFile
from fastapi.middleware.cors import CORSMiddleware
from agents.SongAgent import SongAgent
from models.MoodInputModel import MoodInputModel
from models.MoodOutputModel import MoodOutputModel
from models.RecommendInputModel import RecommendInputModel
from models.RecommendOutputModel import RecommendOutputModel

app = FastAPI()
agent = SongAgent()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://api:8080"],
    allow_methods=["*"],
    allow_headers=["*"],
)

VALID_MOODS = {"happy", "sad", "energetic", "calm", "angry", "romantic", "melancholic", "anxious", "unknown"}
MoodLiteral = Literal["happy", "sad", "energetic", "calm", "angry", "romantic", "melancholic", "anxious", "unknown"]

@app.post("/classify", response_model=MoodOutputModel)
async def classify_mood(input: MoodInputModel) -> MoodOutputModel:
    try:
        mood = await agent.run(f"Classify the mood of this song. lyrics={input.lyrics}, bpm={input.bpm}")
        if not mood:
            raise HTTPException(status_code=500, detail="Model returned empty mood")
        mood = mood.strip().lower()
        mood_value = mood if mood in VALID_MOODS else "unknown"
        return MoodOutputModel(mood=cast(MoodLiteral, mood_value))
    except HTTPException:
        raise
    except Exception as e:
        print(f"ERROR in classify_mood: {e}")
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/recommend", response_model=RecommendOutputModel)
async def recommend_songs(input: RecommendInputModel) -> RecommendOutputModel:
    try:
        available_ids = {str(s.id) for s in input.available_songs}
        raw = await agent.run(f"Recommend songs. listened={input.listened_moods}, available={input.available_songs}")
        if not raw:
            raise HTTPException(status_code=500, detail="Model returned empty recommendations")
        output = raw if isinstance(raw, str) else str(raw)
        song_ids = [s.strip() for s in output.split(",") if s.strip()]
        song_ids = [sid for sid in song_ids if sid in available_ids]
        if not song_ids:
            raise HTTPException(status_code=500, detail=f"Model returned no valid song IDs: {raw}")
        return RecommendOutputModel(song_ids=song_ids)
    except HTTPException:
        raise
    except Exception as e:
        print(f"ERROR in recommend_songs: {e}")
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/bpm")
async def get_bpm(file: UploadFile):
    try:
        audio, sr = librosa.load(file.file)
        tempo, _ = librosa.beat.beat_track(y=audio, sr=sr)
        return {"bpm": round(float(tempo.item()))}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)