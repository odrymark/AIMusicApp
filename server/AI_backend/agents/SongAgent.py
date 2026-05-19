from langchain_mcp_adapters.client import MultiServerMCPClient
from langchain_ollama import ChatOllama
from langchain_classic.agents import AgentExecutor, create_tool_calling_agent
from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder
import asyncio

MCP_SERVER_URL = "http://mcp_server:8001/mcp"

class SongAgent:
    def __init__(self):
        self.llm = ChatOllama(model="song-model", base_url="http://ollama:11434")

    async def run(self, user_input: str) -> str:
        client = MultiServerMCPClient({
            "song-mcp": {
                "url": MCP_SERVER_URL,
                "transport": "streamable_http",
            }
        })
        tools = await client.get_tools()
        prompt = ChatPromptTemplate.from_messages([
            ("system", (
                "You are a music analysis assistant. "
                "Use the available tools to classify song mood and recommend songs based on listening history. "
                "Return ONLY the raw tool output. No explanation, no commentary, no extra text whatsoever.\n\n"
                "Examples of correct behavior:\n"
                "- Tool returns 'sad' -> you return 'sad'\n"
                "- Tool returns 'abc123,def456,ghi789' -> you return 'abc123,def456,ghi789'\n"
                "Examples of incorrect behavior:\n"
                "- Tool returns 'sad' -> you return 'The mood of this song is sad'\n"
                "- Tool returns 'abc123,def456' -> you return 'The recommended songs are abc123,def456'"
            )),
            ("human", "{input}"),
            MessagesPlaceholder("agent_scratchpad"),
        ])
        agent = create_tool_calling_agent(llm=self.llm, tools=tools, prompt=prompt)
        executor = AgentExecutor(agent=agent, tools=tools, verbose=True, handle_parsing_errors=True)
        result = await executor.ainvoke({"input": user_input})
        output = result["output"]
        if isinstance(output, list):
            for block in output:
                if isinstance(block, dict) and block.get("type") == "text":
                    return block["text"].strip()
        if isinstance(output, dict) and output.get("type") == "text":
            return output["text"].strip()
        return str(output).strip()

    def run_sync(self, user_input: str) -> str:
        return asyncio.run(self.run(user_input))