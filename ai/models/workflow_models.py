from datetime import datetime, timezone
from typing import Any, Dict, List, Optional

from pydantic import BaseModel, Field


class MaintenanceObjective(BaseModel):
    maintenance_request_id: int
    objective: str


class PlanStep(BaseModel):
    step_number: int
    agent_name: str
    action: str
    status: str = "PENDING"


class MaintenancePlan(BaseModel):
    steps: List[PlanStep]


class MaintenanceAnalysis(BaseModel):
    category: str
    priority: str
    technician_specialization: str
    estimated_duration_minutes: int
    explanation: str


class TechnicianRecommendation(BaseModel):
    technician_id: int
    technician_name: str
    specialization: str
    scheduled_date: str
    start_time: str
    end_time: str
    reason: str


class ValidationResult(BaseModel):
    is_valid: bool
    errors: List[str] = Field(default_factory=list)
    warnings: List[str] = Field(default_factory=list)


class ManagerApprovalDecision(BaseModel):
    decision: str = "PENDING"
    approved_by: Optional[int] = None
    approved_at: Optional[str] = None
    comment: str = ""


class AuditEntry(BaseModel):
    timestamp: str = Field(
        default_factory=lambda: datetime.now(timezone.utc).isoformat()
    )
    step: str
    agent: str
    action: str
    status: str = "INFO"
    details: Dict[str, Any] = Field(default_factory=dict)


class WorkflowState(BaseModel):
    workflow_id: str
    maintenance_request_id: int
    status: str
    objective: str
    plan: Optional[MaintenancePlan] = None
    analysis: Optional[MaintenanceAnalysis] = None
    technician_recommendation: Optional[TechnicianRecommendation] = None
    validation: Optional[ValidationResult] = None
    approval_required: bool = False
    manager_approval: Optional[ManagerApprovalDecision] = None
    history: List[AuditEntry] = Field(default_factory=list)


class WorkflowResult(BaseModel):
    workflow_id: str
    maintenance_request_id: Optional[int] = None
    status: str
    objective: str
    plan: Optional[MaintenancePlan] = None
    analysis: Optional[MaintenanceAnalysis] = None
    technician_recommendation: Optional[TechnicianRecommendation] = None
    validation: Optional[ValidationResult] = None
    approval_required: bool = False
    manager_approval: Optional[ManagerApprovalDecision] = None
    history: List[AuditEntry] = Field(default_factory=list)
    backend_actions: List[str] = Field(default_factory=list)