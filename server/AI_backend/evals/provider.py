import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

from agents.SongAgent import SongAgent

agent = SongAgent()


def call_api(prompt: str, _options: dict, _context: dict) -> dict[str, str]:
    """
    Promptfoo custom Python provider.
    The rendered prompt string is passed directly to SongAgent.run().
    """
    try:
        output = agent.run(prompt)
        return {"output": output}
    except Exception as e:
        return {"error": str(e)}
