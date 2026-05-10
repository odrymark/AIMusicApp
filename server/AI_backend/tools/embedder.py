from typing import List
from langchain_ollama import OllamaEmbeddings

_embedder = OllamaEmbeddings(model="nomic-embed-text", base_url="http://ollama:11434")

def embed_text(text: str) -> List[float]:
    return _embedder.embed_query(text)
