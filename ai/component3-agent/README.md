# PropMate Component 3 Agentic AI

This is the repository-level AI service for Component 3. It is intentionally **outside** `backend/` and sits beside the existing `property-verification-agent`.

## Four distinct agents

1. **PlannerAgent** — converts the transaction objective into a structured multi-step plan and delegates to the required roles.
2. **DomainAnalysisAgent** — performs transaction-domain analysis using only its allow-listed read tools.
3. **ActionAgent** — prepares a proposed transaction action using its restricted tool set. It cannot bypass the human approval gate.
4. **ValidationAgent** — applies deterministic checks before an approved high-impact action can be treated as successful.

Each role has a different responsibility and controlled tool permissions.

## Assessed workflow

```text
OwnerAgent objective
        ↓
PlannerAgent
        ↓
DomainAnalysisAgent
        ↓
ActionAgent
        ↓
Deterministic validation / approval gate
        ↓
⏸ OwnerAgent approval
        ↓
ValidationAgent checks
        ↓
Auditable final outcome or safe failure
```

Workflow state is persisted in SQLite under `data/agent_workflows.db`. It records the workflow, plan, agent steps, tool calls, validation results, approval state, errors and final outcome.

## API

```text
GET  /health
POST /workflows
GET  /workflows
GET  /workflows/{workflowId}
POST /workflows/{workflowId}/approvals/{approvalId}/decision
```

These routes match the contract used by the ASP.NET Core `AgentWorkflowClient`.

## Run

```bash
cd ai/component3-agent
python -m venv .venv
# activate the venv
pip install -r requirements.txt
cp .env.example .env
# set GOOGLE_API_KEY and INTERNAL_API_KEY
uvicorn app.main:app --host 127.0.0.1 --port 8002
```

## Security

- Gemini credentials remain in this AI service, not ASP.NET Core.
- User/objective fields are Pydantic-validated with `extra="forbid"` for the request model.
- Tools are explicitly allow-listed per agent role.
- Tool inputs and outputs are structured and validated.
- High-impact action proposals always create a pending approval record.
- Only `OwnerAgent` can make the approval decision through the service contract.
- No agent is permitted to execute an unapproved high-impact action.
- Failures are recorded and returned as safe failures rather than silently treated as successful actions.

## Backend transaction execution connector

The current backend package exposes the public user transaction APIs and the AI workflow gateway. The AI service therefore stops at a validated, human-approved action proposal until a dedicated service-to-service transaction execution endpoint is enabled in ASP.NET Core.

That separation is intentional: the AI service must not receive arbitrary database access or bypass the backend's authorization/business rules.
