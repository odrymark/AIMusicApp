import pytest
from unittest.mock import MagicMock, AsyncMock, patch
import mcp_server


class TestMcpServerToolRegistration:
    @pytest.mark.asyncio
    async def test_tools_are_registered(self):
        """Verify that tools are successfully discovered and registered using the public API."""
        tools = await mcp_server.mcp.list_tools()
        registered_names = [tool.name for tool in tools]

        assert "classify_mood" in registered_names
        assert "recommend_songs" in registered_names
        assert "get_song_context" in registered_names


class TestMcpToolInvocations:
    def test_classify_mood_routing(self):
        """Ensure the MCP function wrapper correctly forwards calls to the core classifier tool."""
        with patch("mcp_server.mood_classifier_tool") as mock_tool:
            mock_tool.invoke.return_value = "energetic"

            result = mcp_server.classify_mood(lyrics="Let's rock", bpm=140)

            assert result == "energetic"
            mock_tool.invoke.assert_called_once_with({"lyrics": "Let's rock", "bpm": 140})

    def test_recommend_songs_routing(self):
        """Ensure the MCP function wrapper correctly forwards calls to the core recommendation tool."""
        with patch("mcp_server.recommendation_tool") as mock_tool:
            mock_tool.invoke.return_value = "song_id_1,song_id_2"

            listened = [{"mood": "happy"}]
            available = [{"id": "song_id_1"}]
            result = mcp_server.recommend_songs(listened, available)

            assert result == "song_id_1,song_id_2"
            mock_tool.invoke.assert_called_once_with({
                "listened_moods": listened,
                "available_songs": available
            })


class TestMusicBrainzContextTool:
    @pytest.mark.asyncio
    @patch("mcp_server.asyncio.sleep")
    @patch("mcp_server.httpx.AsyncClient")
    async def test_get_song_context_success(self, mock_client_cls, mock_sleep):
        """Test a successful two-step MusicBrainz API pipeline using standard AsyncMocks."""
        mock_search_resp = MagicMock()
        mock_search_resp.json.return_value = {"recordings": [{"id": "mbid-123-abc"}]}

        mock_lookup_resp = MagicMock()
        mock_lookup_resp.json.return_value = {
            "genres": [
                {"name": "synthpop", "count": 10},
                {"name": "pop", "count": 25},
                {"name": "new wave", "count": 5}
            ]
        }

        mock_client = AsyncMock()
        mock_client.get.side_effect = [mock_search_resp, mock_lookup_resp]
        mock_client_cls.return_value.__aenter__.return_value = mock_client

        result = await mcp_server.get_song_context(title="Blinding Lights", artist="The Weeknd")

        assert result == "genres: pop, synthpop, new wave"
        mock_sleep.assert_called_once_with(1)

    @pytest.mark.asyncio
    @patch("mcp_server.httpx.AsyncClient")
    async def test_get_song_context_no_recordings_found(self, mock_client_cls):
        """Ensure an empty search result safe-exits cleanly."""
        mock_search_resp = MagicMock()
        mock_search_resp.json.return_value = {"recordings": []}

        mock_client = AsyncMock()
        mock_client.get.side_effect = [mock_search_resp]
        mock_client_cls.return_value.__aenter__.return_value = mock_client

        result = await mcp_server.get_song_context(title="Unknown", artist="Nobody")

        assert result == "no genre data available"

    @pytest.mark.asyncio
    @patch("mcp_server.asyncio.sleep")
    @patch("mcp_server.httpx.AsyncClient")
    async def test_get_song_context_no_genres_found(self, mock_client_cls, mock_sleep):
        """Ensure a valid recording with an empty genre property returns the graceful fallback."""
        mock_search_resp = MagicMock()
        mock_search_resp.json.return_value = {"recordings": [{"id": "mbid-blank"}]}

        mock_lookup_resp = MagicMock()
        mock_lookup_resp.json.return_value = {"genres": []}

        mock_client = AsyncMock()
        mock_client.get.side_effect = [mock_search_resp, mock_lookup_resp]
        mock_client_cls.return_value.__aenter__.return_value = mock_client

        result = await mcp_server.get_song_context(title="NoGenreSong", artist="Artist")

        assert result == "no genre data available"