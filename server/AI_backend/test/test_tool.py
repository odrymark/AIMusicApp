import pytest
from unittest.mock import MagicMock, patch


class TestMoodClassifierTool:
    def test_returns_mood_string(self):
        with patch("agents.tools.mood_classifier_tool.client") as mock_client:
            mock_client.chat.return_value = MagicMock(message=MagicMock(content="  sad  "))
            from agents.tools.mood_classifier_tool import mood_classifier_tool
            result = mood_classifier_tool.invoke({"lyrics": "I cry all day", "bpm": 60})
            assert result == "sad"

    def test_strips_whitespace(self):
        with patch("agents.tools.mood_classifier_tool.client") as mock_client:
            mock_client.chat.return_value = MagicMock(message=MagicMock(content="  happy  "))
            from agents.tools.mood_classifier_tool import mood_classifier_tool
            result = mood_classifier_tool.invoke({"lyrics": "I feel great", "bpm": 120})
            assert result == "happy"

    def test_passes_lyrics_and_bpm(self):
        with patch("agents.tools.mood_classifier_tool.client") as mock_client:
            mock_client.chat.return_value = MagicMock(message=MagicMock(content="energetic"))
            from agents.tools.mood_classifier_tool import mood_classifier_tool
            mood_classifier_tool.invoke({"lyrics": "Run run run", "bpm": 180})
            call_args = mock_client.chat.call_args
            assert "Run run run" in str(call_args)
            assert "180" in str(call_args)


class TestRecommendationTool:
    def test_returns_song_ids(self):
        with patch("agents.tools.recommendation_tool.client") as mock_client:
            mock_client.chat.return_value = MagicMock(message=MagicMock(content="abc123,def456,ghi789"))
            from agents.tools.recommendation_tool import recommendation_tool
            result = recommendation_tool.invoke({
                "listened_moods": ["sad", "happy"],
                "available_songs": [{"id": "abc123"}, {"id": "def456"}]
            })
            assert result == "abc123,def456,ghi789"

    def test_strips_whitespace(self):
        with patch("agents.tools.recommendation_tool.client") as mock_client:
            mock_client.chat.return_value = MagicMock(message=MagicMock(content="  abc123  "))
            from agents.tools.recommendation_tool import recommendation_tool
            result = recommendation_tool.invoke({
                "listened_moods": ["sad"],
                "available_songs": [{"id": "abc123"}]
            })
            assert result == "abc123"

    def test_passes_moods_and_songs(self):
        with patch("agents.tools.recommendation_tool.client") as mock_client:
            mock_client.chat.return_value = MagicMock(message=MagicMock(content="abc123"))
            from agents.tools.recommendation_tool import recommendation_tool
            recommendation_tool.invoke({
                "listened_moods": ["sad"],
                "available_songs": [{"id": "abc123"}]
            })
            call_args = mock_client.chat.call_args
            assert "sad" in str(call_args)
            assert "abc123" in str(call_args)