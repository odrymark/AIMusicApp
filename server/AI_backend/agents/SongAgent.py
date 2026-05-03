from langchain_ollama import ChatOllama
from langchain_classic.agents import AgentExecutor, create_tool_calling_agent
from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder
from agents.tools.mood_classifier_tool import mood_classifier_tool
from agents.tools.recommendation_tool import recommendation_tool


class SongAgent:
    def __init__(self):
        llm = ChatOllama(model="song-model", base_url="http://ollama:11434")
        tools = [mood_classifier_tool, recommendation_tool]
        prompt = ChatPromptTemplate.from_messages([
            ("system", (
                "You are a music analysis assistant. "
                "Use the available tools to classify song mood and recommend songs based on listening history. "
                "IMPORTANT: Return ONLY the raw tool output. No explanation, no commentary, no extra text whatsoever.\n\n"
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
        agent = create_tool_calling_agent(llm=llm, tools=tools, prompt=prompt)
        self._executor = AgentExecutor(agent=agent, tools=tools, verbose=True, handle_parsing_errors=True)

    def run(self, user_input: str) -> str:
        result = self._executor.invoke({"input": user_input})
        return result["output"]