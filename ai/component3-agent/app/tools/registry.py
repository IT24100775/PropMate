from typing import Any, Callable
from pydantic import BaseModel, Field

class ToolInput(BaseModel):
    target_type: str
    target_id: int = Field(gt=0)
    objective: str = Field(min_length=1, max_length=1000)

class ToolResult(BaseModel):
    tool_name: str
    succeeded: bool
    data: dict[str, Any]
    error: str | None = None

class ToolRegistry:
    def __init__(self):
        self._tools: dict[str, tuple[set[str], Callable[[ToolInput], ToolResult]]] = {}

    def register(self, name: str, allowed_agents: set[str], fn: Callable[[ToolInput], ToolResult]):
        self._tools[name] = (allowed_agents, fn)

    def execute(self, name: str, agent_role: str, payload: dict[str, Any]) -> ToolResult:
        if name not in self._tools:
            return ToolResult(tool_name=name, succeeded=False, data={}, error="Tool is not allow-listed.")
        allowed_agents, fn = self._tools[name]
        if agent_role not in allowed_agents:
            return ToolResult(tool_name=name, succeeded=False, data={}, error="Agent is not authorized for this tool.")
        try:
            validated = ToolInput.model_validate(payload)
            return fn(validated)
        except Exception as exc:
            return ToolResult(tool_name=name, succeeded=False, data={}, error=f"Tool input/output validation failed: {exc}")

def get_transaction_snapshot(inp: ToolInput) -> ToolResult:
    return ToolResult(tool_name="get_transaction_snapshot", succeeded=True, data={
        "target_type": inp.target_type,
        "target_id": inp.target_id,
        "note": "Snapshot placeholder. Configure BACKEND_INTERNAL_URL and a service-authenticated transaction read endpoint to retrieve live domain data."
    })

def get_negotiation_history(inp: ToolInput) -> ToolResult:
    return ToolResult(tool_name="get_negotiation_history", succeeded=True, data={
        "target_type": inp.target_type,
        "target_id": inp.target_id,
        "history": [],
        "note": "Live history connector is intentionally isolated from the public user API."
    })

def prepare_counter_offer(inp: ToolInput) -> ToolResult:
    return ToolResult(tool_name="prepare_counter_offer", succeeded=True, data={
        "target_type": inp.target_type,
        "target_id": inp.target_id,
        "action": "PREPARE_COUNTER_OFFER",
        "requires_human_approval": True
    })

def record_safe_failure(inp: ToolInput) -> ToolResult:
    return ToolResult(tool_name="record_safe_failure", succeeded=True, data={
        "target_type": inp.target_type,
        "target_id": inp.target_id,
        "action": "SAFE_FAILURE_RECORDED"
    })


def build_registry() -> ToolRegistry:
    r = ToolRegistry()
    r.register("get_transaction_snapshot", {"DomainAnalysisAgent", "ActionAgent", "ValidationAgent"}, get_transaction_snapshot)
    r.register("get_negotiation_history", {"DomainAnalysisAgent", "ActionAgent"}, get_negotiation_history)
    r.register("prepare_counter_offer", {"ActionAgent"}, prepare_counter_offer)
    r.register("record_safe_failure", {"ValidationAgent", "ActionAgent"}, record_safe_failure)
    return r
