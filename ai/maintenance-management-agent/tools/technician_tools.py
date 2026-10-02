import httpx
from datetime import UTC, datetime, timedelta

from pydantic import BaseModel


class TechnicianData(BaseModel):
    id: int
    name: str
    specialization: str
    availability_status: str


class TechnicianToolResult(BaseModel):
    success: bool
    data: object | None = None
    error: str | None = None


class TechnicianTools:
    """
    Allow-listed tools for technician information.

    These tools are READ-ONLY.
    They cannot assign technicians or create schedules.
    """

    BASE_URL = "http://localhost:5235/api"

    SPECIALIZATION_MAP = {
        "PLUMBER": "PLUMBER",
        "PLUMBING": "PLUMBER",
        "ELECTRICIAN": "ELECTRICIAN",
        "ELECTRICAL": "ELECTRICIAN",
        "HVAC": "HVAC TECHNICIAN",
        "HVAC TECHNICIAN": "HVAC TECHNICIAN",
        "AC": "HVAC TECHNICIAN",
        "GENERAL TECHNICIAN": "GENERAL TECHNICIAN",
        "GENERAL": "GENERAL TECHNICIAN",
    }

    ALLOWED_TOOLS = {
        "GetAvailableTechnicians",
        "GetTechnicianAvailability",
        "GetExistingSchedules",
        "FindAvailableSchedule"
    }

    def _normalize_specialization(self, specialization: str | None) -> str:
        if not specialization:
            return ""
        value = str(specialization).strip().upper()
        return self.SPECIALIZATION_MAP.get(value, value)

    def get_available_technicians(
        self,
        specialization: str
    ) -> TechnicianToolResult:

        if not specialization:
            return TechnicianToolResult(
                success=False,
                error="Technician specialization is required."
            )

        try:
            response = httpx.get(
                f"{self.BASE_URL}/Technician/available",
                timeout=10.0
            )

            response.raise_for_status()

            technicians = response.json()
            target_specialization = self._normalize_specialization(specialization)

            matching_technicians = [
                technician
                for technician in technicians
                if self._normalize_specialization(technician.get("specialization", ""))
                == target_specialization
            ]

            return TechnicianToolResult(
                success=True,
                data=matching_technicians
            )

        except httpx.HTTPError as e:
            return TechnicianToolResult(
                success=False,
                error=f"Technician API error: {str(e)}"
            )

    def get_technician_availability(
        self,
        technician_id: int
    ) -> TechnicianToolResult:

        if technician_id <= 0:
            return TechnicianToolResult(
                success=False,
                error="Invalid technician ID."
            )

        try:
            response = httpx.get(
                f"{self.BASE_URL}/Technician/{technician_id}",
                timeout=10.0
            )

            if response.status_code == 404:
                return TechnicianToolResult(
                    success=False,
                    error="Technician not found."
                )

            response.raise_for_status()

            technician = response.json()

            return TechnicianToolResult(
                success=True,
                data={
                    "technician_id": technician.get("id"),
                    "technician_name": technician.get("name"),
                    "specialization": technician.get("specialization"),
                    "availability_status": technician.get("availabilityStatus")
                }
            )

        except httpx.HTTPError as e:
            return TechnicianToolResult(
                success=False,
                error=f"Technician availability API error: {str(e)}"
            )

    def get_existing_schedules(
        self,
        technician_id: int
    ) -> TechnicianToolResult:

        if technician_id <= 0:
            return TechnicianToolResult(
                success=False,
                error="Invalid technician ID."
            )

        try:
            response = httpx.get(
                f"{self.BASE_URL}/Maintenance",
                timeout=10.0
            )
            response.raise_for_status()
            maintenance_requests = response.json()

            historic = []
            for request in maintenance_requests:
                if request.get("assignedTechnicianId") == technician_id or request.get("technicianId") == technician_id:
                    historic.append(request)

            return TechnicianToolResult(success=True, data=historic)
        except httpx.HTTPError as e:
            return TechnicianToolResult(
                success=False,
                error=f"Maintenance history API error: {str(e)}"
            )

    def find_available_schedule(
        self,
        technician_id: int,
        duration_minutes: int
    ) -> TechnicianToolResult:

        if technician_id <= 0:
            return TechnicianToolResult(
                success=False,
                error="Invalid technician ID."
            )

        if duration_minutes <= 0:
            return TechnicianToolResult(
                success=False,
                error="Duration must be greater than zero."
            )

        availability_result = self.get_technician_availability(technician_id)
        if not availability_result.success:
            return availability_result

        technician = availability_result.data
        if technician.get("availability_status", "").upper() != "AVAILABLE":
            return TechnicianToolResult(
                success=False,
                error="Technician is not currently available."
            )

        day_offset = 1
        scheduled_date = (datetime.now(UTC).date() + timedelta(days=day_offset)).isoformat()
        start_time = "10:00"
        end_hour = int(start_time.split(":")[0]) + max(1, (duration_minutes + 59) // 60)
        end_time = f"{end_hour:02d}:00"

        return TechnicianToolResult(
            success=True,
            data={
                "technician_id": technician_id,
                "scheduled_date": scheduled_date,
                "start_time": start_time,
                "end_time": end_time,
                "duration_minutes": duration_minutes
            }
        )