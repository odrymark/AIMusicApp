from langchain_ollama import ChatOllama
from langchain_core.messages import HumanMessage

class MoodAgent:
    def __init__(self, model: str = "song-mood-classifier"):
        self._llm = ChatOllama(model=model)

    def run(self, lyrics: str, bpm: int) -> str:
        response = self._llm.invoke([
            HumanMessage(content=f"lyrics={lyrics}, bpm={bpm}")
        ])
        return response.content.strip()