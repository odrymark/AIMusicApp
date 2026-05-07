import traceback

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

@app.post("/classify", response_model=MoodOutputModel)
async def classify_mood(input: MoodInputModel) -> MoodOutputModel:
    try:
        mood = agent.run(f"Classify the mood of this song. lyrics={input.lyrics}, bpm={input.bpm}")
        if not mood:
            raise HTTPException(status_code=500, detail="Model returned empty mood")
        return MoodOutputModel(mood=mood)
    except HTTPException:
        raise
    except Exception as e:
        print(f"ERROR in classify_mood: {e}")
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/recommend", response_model=RecommendOutputModel)
async def recommend_songs(input: RecommendInputModel) -> RecommendOutputModel:
    try:
        raw = agent.run(f"Recommend songs. listened={input.listened_moods}, available={input.available_songs}")
        if not raw:
            raise HTTPException(status_code=500, detail="Model returned empty recommendations")
        song_ids = [s.strip() for s in raw.split(",") if s.strip()]
        if not song_ids:
            raise HTTPException(status_code=500, detail=f"Model returned non-parseable output: {raw}")
        return RecommendOutputModel(song_ids=song_ids)
    except HTTPException:
        raise
    except Exception as e:
        print(f"ERROR in recommend_songs: {e}")
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/bpm")
async def get_bpm(file: UploadFile):
    audio, sr = librosa.load(file.file)
    tempo, _ = librosa.beat.beat_track(y=audio, sr=sr)
    return {"bpm": round(float(tempo.item()))}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)