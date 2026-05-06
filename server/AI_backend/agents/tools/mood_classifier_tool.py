from langchain_core.tools import tool
import ollama

client = ollama.Client(host="http://localhost:11434")

@tool
def mood_classifier_tool(lyrics: str, bpm: int) -> str:
    """
    Determine the mood of a song given its lyrics and BPM.
    """
    response = client.chat(
        model="song-model",
        messages=[{"role": "user", "content": f"lyrics={lyrics}, bpm={bpm}"}]
    )
    return response.message.content.strip()