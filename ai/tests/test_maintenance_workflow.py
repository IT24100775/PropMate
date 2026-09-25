from types import SimpleNamespace

from agents.planning_agent import PlanningAgent
from agents.maintenance_analysis_agent import MaintenanceAnalysisAgent
from agents.technician_scheduling_agent import TechnicianSchedulingAgent
from agents.validation_agent import ValidationSafetyAgent
from models.workflow_models import (
    MaintenanceAnalysis,
    MaintenanceObjective,
    TechnicianRecommendation,
)
from orchestration.maintenance_workflow import MaintenanceWorkflow


def test_planning_agent_uses_approval_step():
    plan = PlanningAgent().create_plan(
        MaintenanceObjective(
            maintenance_request_id=1,
            objective="There is water leaking from the bathroom pipe.",
        )
    )

    assert [step.agent_name for step in plan.steps] == [
        "MaintenanceAnalysisAgent",
        "TechnicianSchedulingAgent",
        "ValidationSafetyAgent",
        "HumanApproval",
    ]

    assert plan.steps[-1].action.lower().find("approval") >= 0


def test_maintenance_analysis_detects_plumbing_issue():
    analysis = MaintenanceAnalysisAgent().analyze(
        MaintenanceObjective(
            maintenance_request_id=1,
            objective="There is water leaking from the bathroom pipe.",
        )
    )

    assert analysis.category == "PLUMBING"
    assert analysis.priority == "HIGH"
    assert analysis.technician_specialization == "PLUMBER"
    assert analysis.estimated_duration_minutes == 60


def test_validation_agent_accepts_valid_recommendation():
    analysis = MaintenanceAnalysis(
        category="PLUMBING",
        priority="HIGH",
        technician_specialization="PLUMBER",
        estimated_duration_minutes=60,
        explanation="Water leak requires plumbing support.",
    )
    recommendation = TechnicianRecommendation(
        technician_id=5,
        technician_name="Ahmed",
        specialization="PLUMBER",
        scheduled_date="2026-09-25",
        start_time="10:00",
        end_time="11:00",
        reason="Matches specialization and availability.",
    )

    validation = ValidationSafetyAgent().validate(analysis, recommendation)

    assert validation.is_valid is True
    assert validation.errors == []
    assert any("approval" in warning.lower() for warning in validation.warnings)


def test_workflow_supports_backend_specialization_names(monkeypatch):
    workflow = MaintenanceWorkflow()

    monkeypatch.setattr(
        workflow.maintenance_tools,
        "get_maintenance_request",
        lambda request_id: SimpleNamespace(
            success=True,
            data={"id": request_id, "description": "Water leak in the bathroom pipe"},
        ),
    )
    monkeypatch.setattr(
        workflow.technician_tools,
        "get_available_technicians",
        lambda specialization: SimpleNamespace(
            success=True,
            data=[
                {
                    "id": 5,
                    "name": "Ahmed",
                    "specialization": "Plumbing",
                    "availabilityStatus": "AVAILABLE",
                }
            ],
        ),
    )
    monkeypatch.setattr(
        workflow.technician_tools,
        "get_technician_availability",
        lambda technician_id: SimpleNamespace(
            success=True,
            data={
                "technician_id": technician_id,
                "technician_name": "Ahmed",
                "specialization": "Plumbing",
                "availability_status": "AVAILABLE",
            },
        ),
    )
    monkeypatch.setattr(
        workflow.technician_tools,
        "find_available_schedule",
        lambda technician_id, duration_minutes: SimpleNamespace(
            success=True,
            data={
                "technician_id": technician_id,
                "scheduled_date": "2026-09-25",
                "start_time": "10:00",
                "end_time": "11:00",
                "duration_minutes": duration_minutes,
            },
        ),
    )

    result = workflow.run(
        MaintenanceObjective(
            maintenance_request_id=42,
            objective="There is water leaking from the bathroom pipe.",
        )
    )

    assert result.status == "PENDING_MANAGER_APPROVAL"
    assert result.analysis.category == "PLUMBING"
    assert result.technician_recommendation.technician_id == 5
    assert result.technician_recommendation.specialization == "PLUMBER"


def test_workflow_pauses_for_manager_approval(monkeypatch):
    workflow = MaintenanceWorkflow()

    monkeypatch.setattr(
        workflow.maintenance_tools,
        "get_maintenance_request",
        lambda request_id: SimpleNamespace(
            success=True,
            data={"id": request_id, "description": "Water leak in the bathroom pipe"},
        ),
    )
    monkeypatch.setattr(
        workflow.technician_tools,
        "get_available_technicians",
        lambda specialization: SimpleNamespace(
            success=True,
            data=[
                {
                    "id": 5,
                    "name": "Ahmed",
                    "specialization": "PLUMBER",
                    "availabilityStatus": "AVAILABLE",
                }
            ],
        ),
    )
    monkeypatch.setattr(
        workflow.technician_tools,
        "get_technician_availability",
        lambda technician_id: SimpleNamespace(
            success=True,
            data={
                "technician_id": technician_id,
                "technician_name": "Ahmed",
                "specialization": "PLUMBER",
                "availability_status": "AVAILABLE",
            },
        ),
    )
    monkeypatch.setattr(
        workflow.technician_tools,
        "get_existing_schedules",
        lambda technician_id: SimpleNamespace(success=True, data=[]),
    )
    monkeypatch.setattr(
        workflow.technician_tools,
        "find_available_schedule",
        lambda technician_id, duration_minutes: SimpleNamespace(
            success=True,
            data={
                "technician_id": technician_id,
                "scheduled_date": "2026-09-25",
                "start_time": "10:00",
                "end_time": "11:00",
                "duration_minutes": duration_minutes,
            },
        ),
    )

    objective = MaintenanceObjective(
        maintenance_request_id=42,
        objective="There is water leaking from the bathroom pipe.",
    )

    result = workflow.run(objective)

    assert result.status == "PENDING_MANAGER_APPROVAL"
    assert result.approval_required is True
    assert result.analysis.category == "PLUMBING"
    assert result.technician_recommendation.technician_id == 5
    assert result.technician_recommendation.start_time == "10:00"


def test_workflow_uses_backend_description_and_handles_no_matching_technician(monkeypatch):
    workflow = MaintenanceWorkflow()

    monkeypatch.setattr(
        workflow.maintenance_tools,
        "get_maintenance_request",
        lambda request_id: SimpleNamespace(
            success=True,
            data={"id": request_id, "description": "Water leaking from bathroom"},
        ),
    )
    monkeypatch.setattr(
        workflow.technician_tools,
        "get_available_technicians",
        lambda specialization: SimpleNamespace(success=True, data=[]),
    )

    result = workflow.run(
        MaintenanceObjective(
            maintenance_request_id=2,
            objective="Analyze this maintenance request and recommend a suitable technician.",
        )
    )

    assert result.status == "NO_AVAILABLE_TECHNICIAN"
    assert result.analysis.category == "PLUMBING"
    assert result.analysis.technician_specialization == "PLUMBER"
    assert result.technician_recommendation is None
