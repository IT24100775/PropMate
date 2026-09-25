from datetime import UTC, datetime, timedelta

from models.workflow_models import MaintenanceAnalysis, TechnicianRecommendation


class TechnicianSchedulingAgent:
    """
    Technician & Scheduling Agent

    Responsibility:
    - Find a suitable technician.
    - Check availability.
    - Recommend a repair schedule.

    This agent can recommend an assignment,
    but it cannot perform the actual assignment.
    """

    def recommend(
        self,
        analysis: MaintenanceAnalysis,
        available_technicians: list[dict] | None = None,
        schedule_info: dict | None = None,
    ) -> TechnicianRecommendation:
        if not available_technicians:
            available_technicians = []

        specialization_map = {
            "PLUMBER": "PLUMBER",
            "PLUMBING": "PLUMBER",
            "ELECTRICIAN": "ELECTRICIAN",
            "ELECTRICAL": "ELECTRICIAN",
            "HVAC": "HVAC TECHNICIAN",
            "HVAC TECHNICIAN": "HVAC TECHNICIAN",
            "AC": "HVAC TECHNICIAN",
            "GENERAL": "GENERAL TECHNICIAN",
            "GENERAL TECHNICIAN": "GENERAL TECHNICIAN",
        }

        target_specialization = specialization_map.get(
            analysis.technician_specialization.upper(),
            analysis.technician_specialization.upper(),
        )

        matching = []
        for technician in available_technicians:
            specialization = str(technician.get("specialization", "")).upper()
            normalized_specialization = specialization_map.get(specialization, specialization)
            if normalized_specialization == target_specialization:
                matching.append(technician)

        if not matching:
            raise ValueError(
                f"No available technicians match specialization '{analysis.technician_specialization}'."
            )

        selected = matching[0]
        technician_id = int(selected.get("id") or selected.get("technician_id") or 0)
        technician_name = str(selected.get("name") or selected.get("technician_name") or "Unknown")

        if schedule_info:
            schedule_success = getattr(schedule_info, "success", None)
            if schedule_success is None and isinstance(schedule_info, dict):
                schedule_success = schedule_info.get("success")
            schedule_payload = getattr(schedule_info, "data", None)
            if schedule_payload is None and isinstance(schedule_info, dict):
                schedule_payload = schedule_info.get("data")
            if schedule_success or (not isinstance(schedule_info, dict) and schedule_payload is not None):
                if isinstance(schedule_payload, dict):
                    schedule_data = schedule_payload
                elif hasattr(schedule_payload, "__dict__"):
                    schedule_data = schedule_payload.__dict__
                else:
                    schedule_data = {}

                scheduled_date = str(
                    schedule_data.get("scheduled_date")
                    or (datetime.now(UTC).date() + timedelta(days=1)).isoformat()
                )
                start_time = str(schedule_data.get("start_time") or "10:00")
                end_time = str(schedule_data.get("end_time") or "11:00")
            else:
                scheduled_date = (datetime.now(UTC).date() + timedelta(days=1)).isoformat()
                start_time = "10:00"
                end_hour = int(start_time.split(":")[0]) + max(1, (analysis.estimated_duration_minutes + 59) // 60)
                end_time = f"{end_hour:02d}:00"
        else:
            scheduled_date = (datetime.now(UTC).date() + timedelta(days=1)).isoformat()
            start_time = "10:00"
            end_hour = int(start_time.split(":")[0]) + max(1, (analysis.estimated_duration_minutes + 59) // 60)
            end_time = f"{end_hour:02d}:00"

        return TechnicianRecommendation(
            technician_id=technician_id,
            technician_name=technician_name,
            specialization=analysis.technician_specialization,
            scheduled_date=scheduled_date,
            start_time=start_time,
            end_time=end_time,
            reason=(
                f"The technician matches the required specialization and is available. "
                f"Recommended repair window: {scheduled_date} {start_time}-{end_time}."
            ),
        )