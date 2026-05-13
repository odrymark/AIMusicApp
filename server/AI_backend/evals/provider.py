import sys
import os
import asyncio

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from agents.SongAgent import SongAgent

agent = SongAgent()

def call_api(prompt: str, _options: dict, _context: dict) -> dict:
    """
    Promptfoo custom Python provider.
    Uses run_sync to handle the async AgentExecutor loop.
    """
    try:
        output = agent.run_sync(prompt)
        return {"output": output}
    except Exception as e:
        return {"error": str(e)}