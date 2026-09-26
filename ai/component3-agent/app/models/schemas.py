from enum import Enum
from typing import Any
from pydantic import BaseModel, Field, ConfigDict

class TargetType(str, Enum):
    PURCHASE_OFFER = "PurchaseOffer"
    RENTAL_APPLICATION = "RentalApplication"
    PURCHASE_NEGOTIATION = "PurchaseNegotiation"
    RENTAL_NEGOTIATION = "RentalNegotiation"

class WorkflowStatus(str, Enum):
    CREATED = "Created"
    PLANNING = "Planning"
    RUNNING = "Running"
    AWAITING_APPROVAL = "AwaitingApproval"
    COMPLETED = "Completed"
    FAILED = "Failed"
    REJECTED = "Rejected"
    REVISION_REQUESTED = "RevisionRequested"

class StepStatus(str, Enum):
    PENDING = "Pending"
    RUNNING = "Running"
    COMPLETED = "Completed"
    FAILED = "Failed"
    AWAITING_APPROVAL = "AwaitingApproval"
    SKIPPED = "Skipped"

class ApprovalStatus(str, Enum):
    PENDING = "Pending"
    APPROVED = "Approved"
    REJECTED = "Rejected"
    REVISION_REQUESTED = "RevisionRequested"

class StartWorkflowRequest(BaseModel):
    model_config = ConfigDict(extra="forbid")
    objective: str = Field(min_length=1, max_length=1000)
    target_type: TargetType
    target_id: int = Field(gt=0)

class ApprovalDecisionRequest(BaseModel):
    model_config = ConfigDict(extra="forbid")
    decision: ApprovalStatus
    comment: str | None = Field(default=None, max_length=1000)

class PlanStep(BaseModel):
    sequence: int = Field(gt=0)
    agent_role: str
    responsibility: str
    input_contract: str
    output_contract: str

class WorkflowPlan(BaseModel):
    objective: str
    steps: list[PlanStep] = Field(min_length=4)

class ToolCall(BaseModel):
    agent_role: str
    tool_name: str
    input: dict[str, Any]
    output: dict[str, Any] | None = None
    succeeded: bool = True
    error: str | None = None
    duration_ms: int = 0

class ValidationResult(BaseModel):
    valid: bool
    rule: str
    message: str

class AgentStep(BaseModel):
    sequence: int
    agent_role: str
    responsibility: str
    status: StepStatus = StepStatus.PENDING
    input_json: dict[str, Any] = {}
    output_json: dict[str, Any] | None = None
    validation_results: list[ValidationResult] = []
    error: str | None = None
    retry_count: int = 0
    started_at: str | None = None
    completed_at: str | None = None

class Approval(BaseModel):
    id: int
    action_name: str
    payload: dict[str, Any]
    reason: str
    status: ApprovalStatus = ApprovalStatus.PENDING
    decided_by_user_id: int | None = None
    decision_comment: str | None = None
    requested_at: str
    decided_at: str | None = None

class WorkflowRecord(BaseModel):
    id: int
    workflow_id: str
    initiated_by_user_id: int
    initiated_by_role: str
    objective: str
    target_type: TargetType
    target_id: int
    status: WorkflowStatus
    plan: WorkflowPlan | None = None
    steps: list[AgentStep] = []
    tool_calls: list[ToolCall] = []
    approvals: list[Approval] = []
    final_outcome: dict[str, Any] | None = None
    error: str | None = None
    created_at: str
    updated_at: str
    completed_at: str | None = None
