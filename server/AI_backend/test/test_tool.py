import pytest
from unittest.mock import MagicMock, patch

from agents.tools.mood_classifier_tool import mood_classifier_tool
from agents.tools.recommendation_tool import recommendation_tool


@pytest.fixture
def mock_mood_client():
    with patch("agents.tools.mood_classifier_tool.client") as mock_client:
        yield mock_client


@pytest.fixture
def mock_recommend_client():
    with patch("agents.tools.recommendation_tool.client") as mock_client:
        yield mock_client


class TestMoodClassifierTool:
    def test_returns_mood_string(self, mock_mood_client):
        mock_mood_client.chat.return_value = MagicMock(message=MagicMock(content="  sad  "))
        result = mood_classifier_tool.invoke({"lyrics": "I cry all day", "bpm": 60})
        assert result == "sad"

    def test_strips_whitespace(self, mock_mood_client):
        mock_mood_client.chat.return_value = MagicMock(message=MagicMock(content="  happy  "))
        result = mood_classifier_tool.invoke({"lyrics": "I feel great", "bpm": 120})
        assert result == "happy"

    def test_passes_lyrics_and_bpm(self, mock_mood_client):
        mock_mood_client.chat.return_value = MagicMock(message=MagicMock(content="energetic"))
        mood_classifier_tool.invoke({"lyrics": "Run run run", "bpm": 180})
        assert "Run run run" in str(mock_mood_client.chat.call_args)
        assert "180" in str(mock_mood_client.chat.call_args)


class TestRecommendationTool:
    def test_returns_song_ids(self, mock_recommend_client):
        mock_recommend_client.chat.return_value = MagicMock(message=MagicMock(content="abc123,def456,ghi789"))
        result = recommendation_tool.invoke({
            "listened_moods": ["sad", "happy"],
            "available_songs": [{"id": "abc123"}, {"id": "def456"}]
        })
        assert result == "abc123,def456,ghi789"

    def test_strips_whitespace(self, mock_recommend_client):
        mock_recommend_client.chat.return_value = MagicMock(message=MagicMock(content="  abc123  "))
        result = recommendation_tool.invoke({
            "listened_moods": ["sad"],
            "available_songs": [{"id": "abc123"}]
        })
        assert result == "abc123"

    def test_passes_moods_and_songs(self, mock_recommend_client):
        mock_recommend_client.chat.return_value = MagicMock(message=MagicMock(content="abc123"))
        recommendation_tool.invoke({
            "listened_moods": ["sad"],
            "available_songs": [{"id": "abc123"}]
        })
        assert "sad" in str(mock_recommend_client.chat.call_args)
        assert "abc123" in str(mock_recommend_client.chat.call_args)