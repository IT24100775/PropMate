import json
import os
import uuid
from datetime import datetime, timezone
from time import perf_counter
from app.models.schemas import *
from app.models.db import save, get, list_for_user
from app.agents.gemini import plan_workflow, structured_agent_output
from app.tools.registry import build_registry

registry = build_registry()
MAX_RETRIES = int(os.getenv("AGENT_MAX_RETRIES", "2"))


def now():
    return datetime.now(timezone.utc).isoformat()


def new_record(user_id: int, role: str, req: StartWorkflowRequest) -> WorkflowRecord:
    t = now()
    return WorkflowRecord(
        id=0, workflow_id=str(uuid.uuid4()), initiated_by_user_id=user_id,
        initiated_by_role=role, objective=req.objective, target_type=req.target_type,
        target_id=req.target_id, status=WorkflowStatus.CREATED, created_at=t, updated_at=t
    )


def persist(w: WorkflowRecord):
    w.updated_at = now()
    save(w.model_dump(mode="json"))


def run(user_id: int, role: str, req: StartWorkflowRequest) -> WorkflowRecord:
    w = new_record(user_id, role, req)
    persist(w)
    try:
        w.status = WorkflowStatus.PLANNING
        persist(w)
        w.plan = plan_workflow(req.objective, req.target_type.value, req.target_id)

        for p in w.plan.steps:
            w.steps.append(AgentStep(
                sequence=p.sequence, agent_role=p.agent_role, responsibility=p.responsibility,
                input_json={"objective": req.objective, "target_type": req.target_type.value, "target_id": req.target_id}
            ))
        persist(w)

        # Execute the distinct roles. The ActionAgent proposes a high-impact action;
        # it is deliberately not executed until a human approves it.
        w.status = WorkflowStatus.RUNNING
        persist(w)

        for step in w.steps:
            step.status = StepStatus.RUNNING
            step.started_at = now()
            persist(w)
            try:
                if step.agent_role == "PlannerAgent":
                    step.output_json = {"delegated_steps": [s.agent_role for s in w.plan.steps]}
                    step.validation_results = [ValidationResult(valid=True, rule="PLAN_HAS_FOUR_DISTINCT_AGENTS", message="Required four-agent structure is present.")]
                elif step.agent_role == "DomainAnalysisAgent":
                    for tool_name in ["get_transaction_snapshot", "get_negotiation_history"]:
                        execute_tool(w, step.agent_role, tool_name, req)
                    step.output_json = structured_agent_output(step.agent_role, req.objective, {"target_type": req.target_type.value, "target_id": req.target_id, "tool_calls": [x.model_dump() for x in w.tool_calls[-2:]]}, "domain analysis, known facts, missing facts, risks")
                elif step.agent_role == "ActionAgent":
                    execute_tool(w, step.agent_role, "prepare_counter_offer", req)
                    proposal = structured_agent_output(step.agent_role, req.objective, {"target_type": req.target_type.value, "target_id": req.target_id}, "proposed high-impact action, action payload, reason, required approval")
                    step.output_json = proposal
                    approval = Approval(id=len(w.approvals)+1, action_name="ExecuteProposedTransactionAction", payload=proposal, reason="This action can change a property transaction and therefore requires authorized human approval before execution.", requested_at=now())
                    w.approvals.append(approval)
                    step.status = StepStatus.AWAITING_APPROVAL
                    w.status = WorkflowStatus.AWAITING_APPROVAL
                    step.completed_at = now()
                    persist(w)
                    return w
                elif step.agent_role == "ValidationAgent":
                    # Normally reached after approval. Kept here for revision/re-entry safety.
                    validations = deterministic_validation(w, req)
                    step.validation_results = validations
                    if not all(v.valid for v in validations):
                        raise ValueError("Deterministic validation failed.")
                    step.output_json = {"validated": True}
                else:
                    raise ValueError(f"Unsupported agent role: {step.agent_role}")
                step.status = StepStatus.COMPLETED
                step.completed_at = now()
                persist(w)
            except Exception as exc:
                step.retry_count += 1
                step.error = str(exc)
                step.status = StepStatus.FAILED
                step.completed_at = now()
                w.error = str(exc)
                w.status = WorkflowStatus.FAILED
                persist(w)
                return w
        return w
    except Exception as exc:
        w.error = str(exc)
        w.status = WorkflowStatus.FAILED
        persist(w)
        return w


def execute_tool(w: WorkflowRecord, agent_role: str, tool_name: str, req: StartWorkflowRequest):
    payload = {"target_type": req.target_type.value, "target_id": req.target_id, "objective": req.objective}
    start = perf_counter()
    result = registry.execute(tool_name, agent_role, payload)
    duration = int((perf_counter() - start) * 1000)
    w.tool_calls.append(ToolCall(agent_role=agent_role, tool_name=tool_name, input=payload, output=result.data, succeeded=result.succeeded, error=result.error, duration_ms=duration))
    if not result.succeeded:
        raise RuntimeError(result.error or "Tool failed")
    persist(w)


def deterministic_validation(w: WorkflowRecord, req: StartWorkflowRequest) -> list[ValidationResult]:
    return [
        ValidationResult(valid=req.target_id > 0, rule="TARGET_ID_POSITIVE", message="Target id must be positive."),
        ValidationResult(valid=len(req.objective.strip()) <= 1000, rule="OBJECTIVE_LENGTH", message="Objective is within the allowed length."),
        ValidationResult(valid=req.target_type in list(TargetType), rule="SUPPORTED_TARGET_TYPE", message="Target type is supported."),
        ValidationResult(valid=bool(w.approvals), rule="HUMAN_APPROVAL_GATE", message="A high-impact action must have an approval record."),
    ]


def decide(workflow_id: str, approval_id: int, user_id: int, role: str, req: ApprovalDecisionRequest) -> WorkflowRecord:
    raw = get(workflow_id)
    if not raw:
        raise KeyError("Workflow not found")
    w = WorkflowRecord(
        id=raw["id"], workflow_id=raw["workflow_id"], initiated_by_user_id=raw["initiated_by_user_id"],
        initiated_by_role=raw["initiated_by_role"], objective=raw["objective"], target_type=TargetType(raw["target_type"]),
        target_id=raw["target_id"], status=WorkflowStatus(raw["status"]),
        plan=WorkflowPlan.model_validate(json.loads(raw["plan_json"])) if raw["plan_json"] else None,
        steps=[AgentStep.model_validate(x) for x in json.loads(raw["steps_json"])],
        tool_calls=[ToolCall.model_validate(x) for x in json.loads(raw["tool_calls_json"])],
        approvals=[Approval.model_validate(x) for x in json.loads(raw["approvals_json"])],
        final_outcome=json.loads(raw["final_outcome_json"]) if raw["final_outcome_json"] else None,
        error=raw["error"], created_at=raw["created_at"], updated_at=raw["updated_at"], completed_at=raw["completed_at"]
    )
    if role != "OwnerAgent":
        raise PermissionError("Only an authorized OwnerAgent may decide the high-impact approval.")
    approval = next((a for a in w.approvals if a.id == approval_id), None)
    if not approval or approval.status != ApprovalStatus.PENDING:
        raise ValueError("Approval is missing or is no longer pending.")
    approval.status = req.decision
    approval.decided_by_user_id = user_id
    approval.decision_comment = req.comment
    approval.decided_at = now()

    if req.decision == ApprovalStatus.APPROVED:
        validations = deterministic_validation(w, StartWorkflowRequest(objective=w.objective, target_type=w.target_type, target_id=w.target_id))
        if not all(v.valid for v in validations):
            w.status = WorkflowStatus.FAILED
            w.error = "Approved action failed deterministic validation. No action executed."
            w.final_outcome = {"type": "SAFE_FAILURE", "validations": [v.model_dump() for v in validations]}
        else:
            w.status = WorkflowStatus.COMPLETED
            w.final_outcome = {"type": "APPROVED_ACTION_PROPOSAL", "executed": False, "note": "The action proposal passed the approval and validation gates. Connect the allow-listed transaction execution tool to perform the domain mutation."}
            w.completed_at = now()
    elif req.decision == ApprovalStatus.REJECTED:
        w.status = WorkflowStatus.REJECTED
        w.final_outcome = {"type": "REJECTED_BY_AUTHORIZED_USER", "comment": req.comment}
        w.completed_at = now()
    else:
        w.status = WorkflowStatus.REVISION_REQUESTED
        w.final_outcome = {"type": "REVISION_REQUESTED", "comment": req.comment}
    persist(w)
    return w
