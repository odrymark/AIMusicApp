import pytest
from unittest.mock import MagicMock, patch


@pytest.fixture
def agent():
    with patch("agents.SongAgent.ChatOllama"), \
         patch("agents.SongAgent.create_tool_calling_agent"), \
         patch("agents.SongAgent.AgentExecutor") as mock_executor_class:
        mock_executor = MagicMock()
        mock_executor_class.return_value = mock_executor
        from agents.SongAgent import SongAgent
        instance = SongAgent()
        instance._executor = mock_executor
        return instance


class TestSongAgent:
    def test_returns_output(self, agent):
        agent._executor.invoke.return_value = {"output": "sad"}
        result = agent.run("Classify mood")
        assert result == "sad"

    def test_passes_input_to_executor(self, agent):
        agent._executor.invoke.return_value = {"output": "happy"}
        agent.run("some input")
        agent._executor.invoke.assert_called_once_with({"input": "some input"})

    def test_returns_raw_tool_output(self, agent):
        agent._executor.invoke.return_value = {"output": "abc123,def456"}
        result = agent.run("Recommend songs")
        assert result == "abc123,def456"