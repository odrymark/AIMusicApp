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


LISTENED = [{"mood": "sad", "title": "Song A", "artist": "Artist A"}]
AVAILABLE = [{"id": "abc123", "mood": "sad", "title": "Song A", "artist": "Artist A"}]


class TestRecommendationTool:
    def test_returns_song_ids(self):
        with patch("agents.tools.recommendation_tool.client") as mock_client, \
                patch("agents.tools.recommendation_tool.index_songs"), \
                patch("agents.tools.recommendation_tool.embed_text", return_value=[0.1] * 768), \
                patch("agents.tools.recommendation_tool.similarity_search", return_value=[AVAILABLE[0]]):
            mock_client.chat.return_value = MagicMock(message=MagicMock(content="abc123,def456,ghi789"))
            from agents.tools.recommendation_tool import recommendation_tool
            result = recommendation_tool.invoke({
                "listened_moods": LISTENED,
                "available_songs": AVAILABLE
            })
            assert result == "abc123,def456,ghi789"

    def test_strips_whitespace(self):
        with patch("agents.tools.recommendation_tool.client") as mock_client, \
                patch("agents.tools.recommendation_tool.index_songs"), \
                patch("agents.tools.recommendation_tool.embed_text", return_value=[0.1] * 768), \
                patch("agents.tools.recommendation_tool.similarity_search", return_value=[AVAILABLE[0]]):
            mock_client.chat.return_value = MagicMock(message=MagicMock(content="  abc123  "))
            from agents.tools.recommendation_tool import recommendation_tool
            result = recommendation_tool.invoke({
                "listened_moods": LISTENED,
                "available_songs": AVAILABLE
            })
            assert result == "abc123"

    def test_passes_moods_and_songs(self):
        with patch("agents.tools.recommendation_tool.client") as mock_client, \
                patch("agents.tools.recommendation_tool.index_songs"), \
                patch("agents.tools.recommendation_tool.embed_text", return_value=[0.1] * 768), \
                patch("agents.tools.recommendation_tool.similarity_search", return_value=[AVAILABLE[0]]):
            mock_client.chat.return_value = MagicMock(message=MagicMock(content="abc123"))
            from agents.tools.recommendation_tool import recommendation_tool
            recommendation_tool.invoke({
                "listened_moods": LISTENED,
                "available_songs": AVAILABLE
            })
            call_args = mock_client.chat.call_args
            assert "sad" in str(call_args)
            assert "abc123" in str(call_args)