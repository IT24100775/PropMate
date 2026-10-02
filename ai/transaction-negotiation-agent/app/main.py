import json
from fastapi import FastAPI, Header, HTTPException
from dotenv import load_dotenv
from app.models.db import init_db, get, list_for_user
from app.models.schemas import StartWorkflowRequest, ApprovalDecisionRequest, WorkflowRecord, WorkflowStatus
from app.agents.workflow import run, decide
import os

load_dotenv()
init_db()
app = FastAPI(title="PropMate Component 3 Agentic AI", version="1.0.0")


def authorize_internal(api_key: str | None):
    expected = os.getenv("INTERNAL_API_KEY")
    if expected and api_key != expected:
        raise HTTPException(status_code=401, detail="Invalid internal API key.")


def decode(raw: dict) -> WorkflowRecord:
    return WorkflowRecord(
        id=raw["id"], workflow_id=raw["workflow_id"], initiated_by_user_id=raw["initiated_by_user_id"],
        initiated_by_role=raw["initiated_by_role"], objective=raw["objective"], target_type=raw["target_type"],
        target_id=raw["target_id"], status=raw["status"],
        plan=json.loads(raw["plan_json"]) if raw["plan_json"] else None,
        steps=json.loads(raw["steps_json"]), tool_calls=json.loads(raw["tool_calls_json"]), approvals=json.loads(raw["approvals_json"]),
        final_outcome=json.loads(raw["final_outcome_json"]) if raw["final_outcome_json"] else None,
        error=raw["error"], created_at=raw["created_at"], updated_at=raw["updated_at"], completed_at=raw["completed_at"]
    )

@app.get("/health")
def health():
    return {"status": "healthy", "service": "component3-agentic-ai"}

@app.post("/workflows")
def start(req: StartWorkflowRequest, x_user_id: int = Header(...), x_user_role: str = Header(...), x_internal_api_key: str | None = Header(default=None)):
    authorize_internal(x_internal_api_key)
    if x_user_role != "OwnerAgent":
        raise HTTPException(status_code=403, detail="Only OwnerAgent can start a Component 3 AI workflow.")
    return run(x_user_id, x_user_role, req).model_dump(mode="json")

@app.get("/workflows")
def mine(x_user_id: int = Header(...), x_user_role: str = Header(...), x_internal_api_key: str | None = Header(default=None)):
    authorize_internal(x_internal_api_key)
    if x_user_role not in {"OwnerAgent", "Admin"}:
        raise HTTPException(status_code=403, detail="Forbidden.")
    return [decode(x).model_dump(mode="json") for x in list_for_user(x_user_id)]

@app.get("/workflows/{workflow_id}")
def get_workflow(workflow_id: str, x_user_id: int = Header(...), x_user_role: str = Header(...), x_internal_api_key: str | None = Header(default=None)):
    authorize_internal(x_internal_api_key)
    raw = get(workflow_id)
    if not raw:
        raise HTTPException(status_code=404, detail="Workflow not found.")
    if x_user_role != "Admin" and raw["initiated_by_user_id"] != x_user_id:
        raise HTTPException(status_code=403, detail="Forbidden.")
    return decode(raw).model_dump(mode="json")

@app.post("/workflows/{workflow_id}/approvals/{approval_id}/decision")
def approval(workflow_id: str, approval_id: int, req: ApprovalDecisionRequest, x_user_id: int = Header(...), x_user_role: str = Header(...), x_internal_api_key: str | None = Header(default=None)):
    authorize_internal(x_internal_api_key)
    if x_user_role != "OwnerAgent":
        raise HTTPException(status_code=403, detail="Only OwnerAgent may approve this high-impact action.")
    try:
        return decide(workflow_id, approval_id, x_user_id, x_user_role, req).model_dump(mode="json")
    except KeyError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except PermissionError as e:
        raise HTTPException(status_code=403, detail=str(e))
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
