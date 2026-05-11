from typing import List

import ollama
from langchain_core.tools import tool

from tools.embedder import embed_text
from tools.vector_store import index_songs, similarity_search

client = ollama.Client(host="http://ollama:11434")


@tool
def recommendation_tool(listened_moods: List[str], available_songs: List[dict]) -> str:
    """
    Recommend songs based on a user's listening history using RAG.

    listened_moods  – moods the user has listened to (used as the search query).
    available_songs – song catalogue passed in from the .NET backend via the API.

    Returns a comma-separated list of up to 5 song IDs.
    """
    index_songs(available_songs)
    query_embedding = embed_text(" ".join(listened_moods))
    candidate_songs = similarity_search(query_embedding, top_k=10)

    response = client.chat(
        model="song-model",
        messages=[{
            "role": "user",
            "content": f"listened={listened_moods}, available={candidate_songs}",
        }],
    )
    return response.message.content.strip()