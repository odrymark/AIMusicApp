import sys
import os

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from agents.SongAgent import SongAgent

agent = SongAgent()


def call_api(prompt: str, options: dict, context: dict) -> dict:
    """
    Promptfoo custom Python provider.
    The rendered prompt string is passed directly to SongAgent.run().
    """
    try:
        output = agent.run(prompt)
        return {"output": output}
    except Exception as e:
        return {"error": str(e)}