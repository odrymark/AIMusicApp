import ollama
from langchain.tools import BaseTool

class MoodClassifierTool(BaseTool):
    name: str = "MoodClassifier"
    description: str = ("Useful when you need to determine the mood of a song. "
                        "Input should be a string formatted as 'lyrics={lyrics}, bpm={bpm}'.")

    def _run(self, input: str) -> str:
        response = ollama.chat(
            model="song-mood-classifier",
            messages=[
                {"role": "user", "content": input}
            ]
        )
        return response.message.content.strip()

    async def _arun(self, input: str) -> str:
        response = ollama.chat(
            model="song-mood-classifier",
            messages=[
                {"role": "user", "content": input}
            ]
        )
        return response.message.content.strip()