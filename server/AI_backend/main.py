from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from agents.MoodAgent import MoodAgent
from models.SongInputModel import SongInputModel
from models.SongOutputModel import SongOutputModel

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://api:8080"],
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.post("/classify", response_model=SongOutputModel)
async def classify_mood(input: SongInputModel) -> SongOutputModel:
    agent = MoodAgent()
    mood = agent.run(input.lyrics, input.bpm)
    return SongOutputModel(mood=mood)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
