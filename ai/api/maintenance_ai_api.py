from typing import Optional

import httpx
from fastapi import APIRouter, HTTPException
from pydantic import BaseModel

from models.workflow_models import (
    MaintenanceObjective,
    WorkflowResult,
)
from orchestration.maintenance_workflow import MaintenanceWorkflow


router = APIRouter(
    prefix="/api/maintenance-ai",
    tags=["Maintenance AI"],
)

workflow = MaintenanceWorkflow()
BACKEND_URL = "http://localhost:5235/api"


class ManagerApprovalRequest(BaseModel):
    workflow_id: str
    approved_by: int
    approved: bool = True
    comment: str = ""
    technician_id: Optional[int] = None
    scheduled_date: Optional[str] = None
    start_time: Optional[str] = None
    end_time: Optional[str] = None


@router.post(
    "/run",
    response_model=WorkflowResult,
)
def run_maintenance_workflow(
    objective: MaintenanceObjective,
):
    try:
        result = workflow.run(objective)
        return result
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"AI workflow failed: {str(e)}",
        )


@router.post(
    "/approve",
    response_model=WorkflowResult,
)
def approve_maintenance_workflow(request: ManagerApprovalRequest):
    try:
        state = workflow._load_state(request.workflow_id)
        if state is None:
            raise HTTPException(
                status_code=404,
                detail=f"Workflow {request.workflow_id} was not found.",
            )

        if request.approved:
            payload = {
                "approvedBy": request.approved_by,
                "technicianId": request.technician_id,
                "scheduledDate": request.scheduled_date,
                "startTime": request.start_time,
                "endTime": request.end_time,
                "notes": request.comment or "Approved by manager after AI recommendation.",
            }

            backend_response = httpx.post(
                f"{BACKEND_URL}/Maintenance/{state.maintenance_request_id}/ai/approve",
                json=payload,
                timeout=15.0,
            )
            if backend_response.status_code >= 400:
                raise HTTPException(
                    status_code=backend_response.status_code,
                    detail=backend_response.text,
                )

        result = workflow.approve_recommendation(
            workflow_id=request.workflow_id,
            approved_by=request.approved_by,
            approved=request.approved,
            comment=request.comment,
        )
        return result
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"AI approval failed: {str(e)}",
        )