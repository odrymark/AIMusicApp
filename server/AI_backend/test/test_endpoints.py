import pytest
import numpy as np
from fastapi.testclient import TestClient
from unittest.mock import patch, MagicMock, AsyncMock
from io import BytesIO


@pytest.fixture
def client():
    with patch("main.SongAgent"):
        from main import app
        return TestClient(app)


VALID_RECOMMEND_BODY = {
    "listened_moods": [{"mood": "sad", "title": "Song A", "artist": "Artist A"}],
    "available_songs": [
        {"id": "abc123", "mood": "sad", "title": "Song A", "artist": "Artist A"},
        {"id": "def456", "mood": "sad", "title": "Song B", "artist": "Artist B"},
        {"id": "ghi789", "mood": "sad", "title": "Song C", "artist": "Artist C"},
        {"id": "a1", "mood": "sad", "title": "Song D", "artist": "Artist D"},
        {"id": "b2", "mood": "sad", "title": "Song E", "artist": "Artist E"},
        {"id": "c3", "mood": "sad", "title": "Song F", "artist": "Artist F"},
    ]
}


class TestClassifyEndpoint:
    def test_returns_mood(self, client):
        with patch("main.agent.run", new_callable=AsyncMock, return_value="sad"):
            response = client.post("/classify", json={"lyrics": "I cry", "bpm": 60})
            assert response.status_code == 200
            assert response.json()["mood"] == "sad"

    def test_raises_500_on_empty_mood(self, client):
        with patch("main.agent.run", new_callable=AsyncMock, return_value=""):
            response = client.post("/classify", json={"lyrics": "I cry", "bpm": 60})
            assert response.status_code == 500

    def test_raises_422_on_empty_lyrics(self, client):
        response = client.post("/classify", json={"lyrics": "", "bpm": 60})
        assert response.status_code == 422

    def test_raises_422_on_invalid_bpm(self, client):
        response = client.post("/classify", json={"lyrics": "I cry", "bpm": -1})
        assert response.status_code == 422

    def test_raises_500_on_exception(self, client):
        with patch("main.agent.run", new_callable=AsyncMock, side_effect=Exception("ollama down")):
            response = client.post("/classify", json={"lyrics": "I cry", "bpm": 60})
            assert response.status_code == 500


class TestRecommendEndpoint:
    def test_returns_song_ids(self, client):
        with patch("main.agent.run", new_callable=AsyncMock, return_value="abc123,def456,ghi789"):
            response = client.post("/recommend", json=VALID_RECOMMEND_BODY)
            assert response.status_code == 200
            assert response.json()["song_ids"] == ["abc123", "def456", "ghi789"]

    def test_splits_comma_separated_ids(self, client):
        with patch("main.agent.run", new_callable=AsyncMock, return_value="a1, b2, c3"):
            response = client.post("/recommend", json=VALID_RECOMMEND_BODY)
            assert response.json()["song_ids"] == ["a1", "b2", "c3"]

    def test_raises_422_on_empty_moods(self, client):
        response = client.post("/recommend", json={
            "listened_moods": [],
            "available_songs": [{"id": "abc123", "mood": "sad", "title": "Song A", "artist": "Artist A"}]
        })
        assert response.status_code == 422

    def test_raises_422_on_empty_songs(self, client):
        response = client.post("/recommend", json={
            "listened_moods": [{"mood": "sad", "title": "Song A", "artist": "Artist A"}],
            "available_songs": []
        })
        assert response.status_code == 422

    def test_raises_500_on_empty_response(self, client):
        with patch("main.agent.run", new_callable=AsyncMock, return_value=""):
            response = client.post("/recommend", json=VALID_RECOMMEND_BODY)
            assert response.status_code == 500

    def test_raises_500_on_exception(self, client):
        with patch("main.agent.run", new_callable=AsyncMock, side_effect=Exception("ollama down")):
            response = client.post("/recommend", json=VALID_RECOMMEND_BODY)
            assert response.status_code == 500


class TestBpmEndpoint:
    def _make_audio_file(self) -> BytesIO:
        return BytesIO(b"fake-audio-data")

    def test_returns_bpm(self, client):
        with patch("main.librosa.load", return_value=(np.zeros(1000), 22050)), \
             patch("main.librosa.beat.beat_track", return_value=(np.array(120.0), None)):
            response = client.post(
                "/bpm",
                files={"file": ("test.mp3", self._make_audio_file(), "audio/mpeg")},
            )
            assert response.status_code == 200
            assert response.json()["bpm"] == 120

    def test_bpm_is_rounded(self, client):
        with patch("main.librosa.load", return_value=(np.zeros(1000), 22050)), \
             patch("main.librosa.beat.beat_track", return_value=(np.array(118.7), None)):
            response = client.post(
                "/bpm",
                files={"file": ("test.mp3", self._make_audio_file(), "audio/mpeg")},
            )
            assert response.json()["bpm"] == 119

    def test_raises_422_on_missing_file(self, client):
        response = client.post("/bpm")
        assert response.status_code == 422

    def test_raises_500_on_librosa_error(self, client):
        with patch("main.librosa.load", side_effect=Exception("corrupt audio")):
            response = client.post(
                "/bpm",
                files={"file": ("test.mp3", self._make_audio_file(), "audio/mpeg")},
            )
            assert response.status_code == 500