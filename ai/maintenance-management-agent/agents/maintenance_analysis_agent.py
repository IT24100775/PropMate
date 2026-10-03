import json
import os
import re

from models.workflow_models import MaintenanceAnalysis, MaintenanceObjective


class MaintenanceAnalysisAgent:
    """
    Maintenance Analysis Agent

    Responsibility:
    - Analyze the maintenance issue.
    - Determine the likely category.
    - Determine priority.
    - Identify required technician specialization.
    - Estimate repair duration.

    This agent does NOT assign a technician or modify the database.
    """

    def _call_gemini(self, objective: MaintenanceObjective) -> dict | None:
        api_key = os.getenv("GEMINI_API_KEY")
        if not api_key:
            return None

        try:
            from google import genai

            client = genai.Client(api_key=api_key)
            prompt = (
                "You are a property maintenance classification assistant. "
                "Return ONLY valid JSON with fields: category, priority, "
                "technician_specialization, estimated_duration_minutes, explanation. "
                "Use values from this allowed set: category in ['PLUMBING','ELECTRICAL','HVAC','GENERAL']; "
                "priority in ['LOW','MEDIUM','HIGH','CRITICAL']; "
                "technician_specialization in ['PLUMBER','ELECTRICIAN','HVAC TECHNICIAN','GENERAL TECHNICIAN']; "
                f"Description: {objective.objective}"
            )
            response = client.models.generate_content(
                model="gemini-2.5-flash",
                contents=prompt,
            )
            text = getattr(response, "text", None) or str(response)
            match = re.search(r"\{.*\}", text, re.DOTALL)
            if not match:
                return None
            return json.loads(match.group(0))
        except Exception:
            return None

    def _fallback_analysis(self, description: str) -> MaintenanceAnalysis:
        text = description.lower()

        if "water" in text or "leak" in text or "pipe" in text:
            category = "PLUMBING"
            priority = "HIGH"
            specialization = "PLUMBER"
            duration = 60
            explanation = (
                "The issue appears to involve a water leak or plumbing system "
                "failure, so plumbing assistance is required."
            )
        elif "electric" in text or "power" in text or "outlet" in text or "circuit" in text:
            category = "ELECTRICAL"
            priority = "HIGH"
            specialization = "ELECTRICIAN"
            duration = 60
            explanation = (
                "The issue appears to involve an electrical problem, so an "
                "electrician is required."
            )
        elif "air conditioner" in text or "ac " in text or "hvac" in text:
            category = "HVAC"
            priority = "MEDIUM"
            specialization = "HVAC TECHNICIAN"
            duration = 90
            explanation = "The issue appears to involve the HVAC system."
        else:
            category = "GENERAL"
            priority = "MEDIUM"
            specialization = "GENERAL TECHNICIAN"
            duration = 60
            explanation = (
                "The issue could not be assigned to a more specific maintenance "
                "category, so a general technician is recommended."
            )

        return MaintenanceAnalysis(
            category=category,
            priority=priority,
            technician_specialization=specialization,
            estimated_duration_minutes=duration,
            explanation=explanation,
        )

    def analyze(self, objective: MaintenanceObjective) -> MaintenanceAnalysis:
        description = objective.objective or ""

        gemini_result = self._call_gemini(objective)
        if gemini_result:
            try:
                cleaned = {
                    "category": str(gemini_result.get("category", "GENERAL")).upper(),
                    "priority": str(gemini_result.get("priority", "MEDIUM")).upper(),
                    "technician_specialization": str(
                        gemini_result.get("technician_specialization", "GENERAL TECHNICIAN")
                    ).upper(),
                    "estimated_duration_minutes": int(
                        gemini_result.get("estimated_duration_minutes", 60)
                    ),
                    "explanation": str(gemini_result.get("explanation", "")),
                }
                analysis = MaintenanceAnalysis.model_validate(cleaned)
                if analysis.category and analysis.priority and analysis.technician_specialization:
                    return analysis
            except Exception:
                pass

        return self._fallback_analysis(description)