import json
from datetime import datetime, timezone
from pathlib import Path

from agents.planning_agent import PlanningAgent
from agents.maintenance_analysis_agent import MaintenanceAnalysisAgent
from agents.technician_scheduling_agent import TechnicianSchedulingAgent
from agents.validation_agent import ValidationSafetyAgent
from models.workflow_models import (
    AuditEntry,
    MaintenanceObjective,
    ManagerApprovalDecision,
    WorkflowResult,
    WorkflowState,
)
from tools.maintenance_tools import MaintenanceTools
from tools.technician_tools import TechnicianTools


class MaintenanceWorkflow:
    """
    Main orchestrator for the PropMate maintenance workflow.

    The orchestrator coordinates the four agents
    and controls the workflow sequence.
    """

    def __init__(self, state_path: str | None = None):
        self.planning_agent = PlanningAgent()
        self.analysis_agent = MaintenanceAnalysisAgent()
        self.scheduling_agent = TechnicianSchedulingAgent()
        self.validation_agent = ValidationSafetyAgent()

        self.maintenance_tools = MaintenanceTools()
        self.technician_tools = TechnicianTools()
        self.state_path = Path(state_path) if state_path else Path(__file__).resolve().parent.parent / "workflow_state.json"

    def _create_workflow_id(self) -> str:
        timestamp = datetime.now(timezone.utc).strftime("%Y%m%d%H%M%S")
        return f"WF-{timestamp}"

    def _record_event(self, state: WorkflowState, step: str, agent: str, action: str, status: str, **details) -> None:
        entry = AuditEntry(
            step=step,
            agent=agent,
            action=action,
            status=status,
            details=details,
        )
        state.history.append(entry)

    def _persist_state(self, state: WorkflowState) -> None:
        self.state_path.parent.mkdir(parents=True, exist_ok=True)
        payload = state.model_dump(mode="json")
        self.state_path.write_text(json.dumps(payload, indent=2), encoding="utf-8")

    def _load_state(self, workflow_id: str) -> WorkflowState | None:
        if not self.state_path.exists():
            return None

        try:
            data = json.loads(self.state_path.read_text(encoding="utf-8"))
        except json.JSONDecodeError:
            return None

        if data.get("workflow_id") != workflow_id:
            return None

        return WorkflowState.model_validate(data)

    def run(self, objective: MaintenanceObjective) -> WorkflowResult:
        workflow_id = self._create_workflow_id()
        plan = self.planning_agent.create_plan(objective)
        state = WorkflowState(
            workflow_id=workflow_id,
            maintenance_request_id=objective.maintenance_request_id,
            status="INITIALIZING",
            objective=objective.objective,
            plan=plan,
            approval_required=False,
            history=[],
        )

        self._record_event(state, "Planning", "PlanningAgent", "Create workflow plan", "PENDING", plan=plan.model_dump(mode="json"))
        state.status = "PLANNED"
        self._persist_state(state)

        maintenance_result = self.maintenance_tools.get_maintenance_request(objective.maintenance_request_id)
        if not maintenance_result.success:
            state.status = "FAILED"
            self._record_event(state, "Data Retrieval", "MaintenanceTools", "Read maintenance request", "FAILED", error=maintenance_result.error)
            self._persist_state(state)
            return WorkflowResult(
                workflow_id=workflow_id,
                status="FAILED",
                objective=objective.objective,
                plan=plan,
                history=state.history,
                approval_required=False,
            )

        for step in plan.steps:
            if step.agent_name == "MaintenanceAnalysisAgent":
                step.status = "COMPLETED"
                break

        maintenance_data = maintenance_result.data if isinstance(maintenance_result.data, dict) else {}
        analysis_objective = MaintenanceObjective(
            maintenance_request_id=objective.maintenance_request_id,
            objective=str(maintenance_data.get("description") or objective.objective),
        )
        analysis = self.analysis_agent.analyze(analysis_objective)
        state.analysis = analysis
        self._record_event(
            state,
            "Analysis",
            "MaintenanceAnalysisAgent",
            "Analyze maintenance issue and classify it",
            "COMPLETED",
            category=analysis.category,
            priority=analysis.priority,
            specialization=analysis.technician_specialization,
        )

        technicians_result = self.technician_tools.get_available_technicians(analysis.technician_specialization)
        if not technicians_result.success:
            state.status = "FAILED"
            self._record_event(state, "Scheduling", "TechnicianSchedulingAgent", "Find matching technicians", "FAILED", error=technicians_result.error)
            self._persist_state(state)
            return WorkflowResult(
                workflow_id=workflow_id,
                status="FAILED",
                objective=objective.objective,
                plan=plan,
                analysis=analysis,
                history=state.history,
                approval_required=False,
            )

        available_technicians = technicians_result.data or []
        if not available_technicians:
            error = (
                "No available technicians match specialization "
                f"'{analysis.technician_specialization}'."
            )
            state.status = "NO_AVAILABLE_TECHNICIAN"
            plan.steps[1].status = "FAILED"
            self._record_event(
                state,
                "Scheduling",
                "TechnicianSchedulingAgent",
                "Find matching technicians",
                "FAILED",
                error=error,
            )
            self._persist_state(state)
            return WorkflowResult(
                workflow_id=workflow_id,
                maintenance_request_id=objective.maintenance_request_id,
                status="NO_AVAILABLE_TECHNICIAN",
                objective=objective.objective,
                plan=plan,
                analysis=analysis,
                history=state.history,
                approval_required=False,
            )

        schedule_result = self.technician_tools.find_available_schedule(
            int(available_technicians[0].get("id") or available_technicians[0].get("technician_id") or 0),
            analysis.estimated_duration_minutes,
        )
        schedule_payload = schedule_result.model_dump() if hasattr(schedule_result, "model_dump") else schedule_result
        recommendation = self.scheduling_agent.recommend(
            analysis,
            available_technicians,
            schedule_info=schedule_payload,
        )
        state.technician_recommendation = recommendation
        self._record_event(
            state,
            "Scheduling",
            "TechnicianSchedulingAgent",
            "Recommend technician and schedule",
            "COMPLETED",
            technician_id=recommendation.technician_id,
            technician_name=recommendation.technician_name,
            date=recommendation.scheduled_date,
            start_time=recommendation.start_time,
            end_time=recommendation.end_time,
        )

        validation = self.validation_agent.validate(analysis, recommendation, available_technicians)
        state.validation = validation
        self._record_event(
            state,
            "Validation",
            "ValidationSafetyAgent",
            "Validate AI outputs using deterministic rules",
            "COMPLETED" if validation.is_valid else "FAILED",
            is_valid=validation.is_valid,
            errors=validation.errors,
            warnings=validation.warnings,
        )

        if not validation.is_valid:
            state.status = "VALIDATION_FAILED"
            self._persist_state(state)
            return WorkflowResult(
                workflow_id=workflow_id,
                status="VALIDATION_FAILED",
                objective=objective.objective,
                plan=plan,
                analysis=analysis,
                technician_recommendation=recommendation,
                validation=validation,
                history=state.history,
                approval_required=False,
            )

        plan.steps[3].status = "PENDING"
        state.plan = plan
        state.status = "PENDING_MANAGER_APPROVAL"
        state.approval_required = True
        state.manager_approval = ManagerApprovalDecision(
            decision="PENDING",
            comment="Manager approval required before assignment or schedule creation.",
        )
        result = WorkflowResult(
            workflow_id=workflow_id,
            status="PENDING_MANAGER_APPROVAL",
            objective=objective.objective,
            plan=plan,
            analysis=analysis,
            technician_recommendation=recommendation,
            validation=validation,
            approval_required=True,
            manager_approval=state.manager_approval,
            history=state.history,
            backend_actions=[
                "POST /api/Maintenance/{id}/assign",
                "POST /api/Maintenance/{id}/schedule",
            ],
        )

        self._persist_state(state)
        return result

    def approve_recommendation(
        self,
        workflow_id: str,
        approved_by: int,
        approved: bool = True,
        comment: str = "",
    ) -> WorkflowResult:
        state = self._load_state(workflow_id)
        if state is None:
            raise ValueError(f"Workflow {workflow_id} was not found.")

        if approved:
            state.status = "APPROVED_PENDING_BACKEND_EXECUTION"
            state.manager_approval = ManagerApprovalDecision(
                decision="APPROVED",
                approved_by=approved_by,
                approved_at=datetime.now(timezone.utc).isoformat(),
                comment=comment or "Approved by manager. Backend assignment and scheduling can proceed.",
            )
            self._record_event(
                state,
                "Approval",
                "HumanApproval",
                "Manager approved recommendation",
                "APPROVED",
                approved_by=approved_by,
                comment=state.manager_approval.comment,
            )
            state.approval_required = False
            self._persist_state(state)
            return WorkflowResult(
                workflow_id=workflow_id,
                status="APPROVED_PENDING_BACKEND_EXECUTION",
                objective=state.objective,
                plan=state.plan,
                analysis=state.analysis,
                technician_recommendation=state.technician_recommendation,
                validation=state.validation,
                approval_required=False,
                manager_approval=state.manager_approval,
                history=state.history,
                backend_actions=[
                    "POST /api/Maintenance/{id}/assign",
                    "POST /api/Maintenance/{id}/schedule",
                ],
            )

        state.status = "REJECTED_BY_MANAGER"
        state.manager_approval = ManagerApprovalDecision(
            decision="REJECTED",
            approved_by=approved_by,
            approved_at=datetime.now(timezone.utc).isoformat(),
            comment=comment or "Manager rejected the recommended assignment and schedule.",
        )
        state.approval_required = False
        self._record_event(
            state,
            "Approval",
            "HumanApproval",
            "Manager rejected recommendation",
            "REJECTED",
            approved_by=approved_by,
            comment=state.manager_approval.comment,
        )
        self._persist_state(state)
        return WorkflowResult(
            workflow_id=workflow_id,
            status="REJECTED_BY_MANAGER",
            objective=state.objective,
            plan=state.plan,
            analysis=state.analysis,
            technician_recommendation=state.technician_recommendation,
            validation=state.validation,
            approval_required=False,
            manager_approval=state.manager_approval,
            history=state.history,
            backend_actions=[],
        )