from datetime import UTC, datetime

from models.workflow_models import MaintenanceAnalysis, TechnicianRecommendation, ValidationResult


class ValidationSafetyAgent:
    """
    Validation & Safety Agent

    Responsibility:
    - Validate agent outputs.
    - Apply deterministic business rules.
    - Reject unsafe or invalid recommendations.
    - Ensure high-impact actions require human approval.
    """

    @staticmethod
    def _normalize_specialization(value: str | None) -> str:
        if not value:
            return ""
        normalized = str(value).strip().upper()
        mapping = {
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
        return mapping.get(normalized, normalized)

    def validate(
        self,
        analysis: MaintenanceAnalysis,
        recommendation: TechnicianRecommendation,
        available_technicians: list[dict] | None = None,
    ) -> ValidationResult:
        errors: list[str] = []
        warnings: list[str] = []

        valid_categories = {"PLUMBING", "ELECTRICAL", "HVAC", "GENERAL"}
        valid_priorities = {"LOW", "MEDIUM", "HIGH", "CRITICAL"}
        valid_specializations = {
            "PLUMBER",
            "ELECTRICIAN",
            "HVAC TECHNICIAN",
            "GENERAL TECHNICIAN",
        }

        if not analysis.category:
            errors.append("Maintenance category is missing.")
        elif analysis.category.upper() not in valid_categories:
            errors.append("Category must be one of the valid maintenance categories.")

        if not analysis.priority:
            errors.append("Maintenance priority is missing.")
        elif analysis.priority.upper() not in valid_priorities:
            errors.append("Priority must be one of LOW, MEDIUM, HIGH, or CRITICAL.")

        if not analysis.technician_specialization:
            errors.append("Technician specialization is missing.")
        elif analysis.technician_specialization.upper() not in valid_specializations:
            errors.append("Technician specialization must be valid.")

        if analysis.estimated_duration_minutes <= 0:
            errors.append("Repair duration must be greater than zero.")

        if recommendation.technician_id <= 0:
            errors.append("Invalid technician ID.")

        if not recommendation.technician_name:
            errors.append("Technician name is missing.")

        if not recommendation.scheduled_date:
            errors.append("Scheduled date is missing.")
        else:
            try:
                scheduled_date = datetime.strptime(recommendation.scheduled_date, "%Y-%m-%d").date()
                if scheduled_date < datetime.now(UTC).date():
                    errors.append("Schedule must not be in the past.")
            except ValueError:
                errors.append("Scheduled date must use the YYYY-MM-DD format.")

        if not recommendation.start_time or not recommendation.end_time:
            errors.append("Schedule times are missing.")
        else:
            try:
                start = datetime.strptime(recommendation.start_time, "%H:%M").time()
                end = datetime.strptime(recommendation.end_time, "%H:%M").time()
                if start >= end:
                    errors.append("Schedule start time must be before end time.")
            except ValueError:
                errors.append("Schedule times must use the HH:MM format.")

        if available_technicians is not None:
            ids = {int(item.get("id") or item.get("technician_id") or 0) for item in available_technicians}
            if recommendation.technician_id not in ids:
                errors.append("Recommended technician must exist and be available.")

            matched_specialization = self._normalize_specialization(
                next(
                    (
                        item.get("specialization", "")
                        for item in available_technicians
                        if int(item.get("id") or item.get("technician_id") or 0)
                        == recommendation.technician_id
                    ),
                    "",
                )
            )
            if matched_specialization and self._normalize_specialization(recommendation.specialization) != matched_specialization:
                errors.append("Technician specialization must match the required specialization.")

        if self._normalize_specialization(recommendation.specialization) != self._normalize_specialization(analysis.technician_specialization):
            errors.append("Technician recommendation specialization must match the analysis output.")

        warnings.append(
            "Technician assignment and schedule creation require manager approval."
        )

        return ValidationResult(
            is_valid=len(errors) == 0,
            errors=errors,
            warnings=warnings,
        )