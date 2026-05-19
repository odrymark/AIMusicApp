import sys
import os
import asyncio

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from agents.SongAgent import SongAgent

def call_api(prompt: str, _options: dict, _context: dict) -> dict:
    try:
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        try:
            agent = SongAgent()
            output = loop.run_until_complete(agent.run(prompt))
            return {"output": output}
        finally:
            loop.close()
    except Exception as e:
        return {"error": str(e)}