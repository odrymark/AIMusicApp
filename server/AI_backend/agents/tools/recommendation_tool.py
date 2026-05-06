from langchain_core.tools import tool
from typing import List
import ollama

client = ollama.Client(host="http://localhost:11434")

@tool
def recommendation_tool(listened_moods: List[str], available_songs: List[dict]) -> str:
    """
    Recommend songs based on a user's listening history.
    listened_moods is a list of mood strings the user has listened to.
    available_songs is a list of dicts with song data to recommend from.
    Returns a comma-separated list of song IDs.
    """
    response = client.chat(
        model="song-model",
        messages=[{"role": "user", "content": f"listened={listened_moods}, available={available_songs}"}]
    )
    return response.message.content.strip()