from typing import List
import numpy as np
from tools.embedder import embed_text

_store: List[dict] = []


def index_songs(songs: List[dict]) -> None:
    global _store
    _store = []
    for song in songs:
        text = song.get("mood", "unknown")
        embedding = embed_text(text)
        _store.append({"text": text, "embedding": embedding, "song": song})
    print(f"[RAG] Indexed {len(songs)} songs in memory.")


def _cosine_similarity(vec_a: List[float], vec_b: List[float]) -> float:
    a = np.array(vec_a)
    b = np.array(vec_b)
    denominator = np.linalg.norm(a) * np.linalg.norm(b)
    if denominator == 0:
        return 0.0
    return float(np.dot(a, b) / denominator)


def similarity_search(query_embedding: List[float], top_k: int = 10) -> List[dict]:
    """Return the top_k most similar songs by cosine similarity."""
    scored = [
        (entry["song"], _cosine_similarity(query_embedding, entry["embedding"]))
        for entry in _store
    ]
    scored.sort(key=lambda x: x[1], reverse=True)
    return [song for song, score in scored[:top_k]]
