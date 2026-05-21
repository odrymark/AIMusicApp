import pytest
from unittest.mock import MagicMock, AsyncMock, patch


@pytest.fixture
def agent():
    mock_client = MagicMock()
    mock_client.get_tools = AsyncMock(return_value=[])

    with patch("agents.SongAgent.MultiServerMCPClient", return_value=mock_client), \
            patch("agents.SongAgent.ChatOllama"), \
            patch("agents.SongAgent.create_tool_calling_agent"), \
            patch("agents.SongAgent.AgentExecutor") as mock_executor_class:
        mock_executor = MagicMock()
        mock_executor_class.return_value = mock_executor

        from agents.SongAgent import SongAgent
        instance = SongAgent()
        instance._executor = mock_executor

        yield instance


class TestSongAgent:
    @pytest.mark.asyncio
    async def test_returns_output(self, agent):
        agent._executor.ainvoke = AsyncMock(return_value={"output": "sad"})
        result = await agent.run("Classify mood")
        assert result == "sad"

    @pytest.mark.asyncio
    async def test_passes_input_to_executor(self, agent):
        agent._executor.ainvoke = AsyncMock(return_value={"output": "happy"})
        await agent.run("some input")
        agent._executor.ainvoke.assert_called_once_with({"input": "some input"})

    @pytest.mark.asyncio
    async def test_returns_raw_tool_output(self, agent):
        agent._executor.ainvoke = AsyncMock(return_value={"output": "abc123,def456"})
        result = await agent.run("Recommend songs")
        assert result == "abc123,def456"