import asyncio
import httpx
from fastmcp import FastMCP
from agents.tools.mood_classifier_tool import mood_classifier_tool
from agents.tools.recommendation_tool import recommendation_tool

mcp = FastMCP("song-mcp")

@mcp.tool()
def classify_mood(lyrics: str, bpm: int) -> str:
    """Classify the mood of a song given its lyrics and BPM."""
    return mood_classifier_tool.invoke({"lyrics": lyrics, "bpm": bpm})

@mcp.tool()
def recommend_songs(listened_moods: list[dict], available_songs: list[dict]) -> str:
    """Recommend songs based on a user's listening history."""
    return recommendation_tool.invoke({"listened_moods": listened_moods, "available_songs": available_songs})

@mcp.tool()
async def get_song_context(title: str, artist: str) -> str:
    """Fetch genre tags for a specific song from MusicBrainz."""
    async with httpx.AsyncClient() as client:
        search_response = await client.get(
            "https://musicbrainz.org/ws/2/recording/",
            params={"query": f"recording:{title} AND artist:{artist}", "fmt": "json", "limit": 1},
            headers={"User-Agent": "SongApp/1.0 ( musicapp@gmail.com )"}
        )
        data = search_response.json()
        recordings = data.get("recordings", [])
        if not recordings:
            return "no genre data available"

        mbid = recordings[0]["id"]
        await asyncio.sleep(1)

        lookup_response = await client.get(
            f"https://musicbrainz.org/ws/2/recording/{mbid}",
            params={"inc": "genres", "fmt": "json"},
            headers={"User-Agent": "SongApp/1.0 ( musicapp@gmail.com )"}
        )
        recording = lookup_response.json()
        genres = recording.get("genres", [])
        if not genres:
            return "no genre data available"

        genre_names = [g["name"] for g in sorted(genres, key=lambda x: x.get("count", 0), reverse=True)[:5]]
        return f"genres: {', '.join(genre_names)}"

if __name__ == "__main__":
    mcp.run(transport="streamable-http", host="0.0.0.0", port=8001)