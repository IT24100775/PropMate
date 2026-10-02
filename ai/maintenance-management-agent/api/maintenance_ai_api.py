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
            if request.technician_id is None:
                raise HTTPException(status_code=400, detail="A technician must be selected.")
            selected_technician = next(
                (
                    technician
                    for technician in state.available_technicians
                    if int(technician.get("id") or technician.get("technician_id") or 0)
                    == request.technician_id
                ),
                None,
            )
            if selected_technician is None:
                raise HTTPException(
                    status_code=400,
                    detail="Selected technician is not available for this recommendation.",
                )

            recommendation = state.technician_recommendation
            if recommendation is None:
                raise HTTPException(status_code=400, detail="Workflow has no technician recommendation.")

            scheduled_date = request.scheduled_date or recommendation.scheduled_date
            if len(scheduled_date) == 10:
                scheduled_date = f"{scheduled_date}T00:00:00Z"

            payload = {
                "approvedBy": request.approved_by,
                "technicianId": request.technician_id,
                "scheduledDate": scheduled_date,
                "startTime": request.start_time or recommendation.start_time,
                "endTime": request.end_time or recommendation.end_time,
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
            technician_id=request.technician_id,
        )
        return result
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"AI approval failed: {str(e)}",
        )