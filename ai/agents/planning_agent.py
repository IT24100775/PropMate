from models.workflow_models import MaintenanceObjective, MaintenancePlan, PlanStep


class PlanningAgent:
    """
    Planning Agent

    Responsibility:
    - Receive the maintenance objective.
    - Create a structured multi-step plan.
    - Delegate work to the appropriate agents.
    """

    def create_plan(
        self,
        objective: MaintenanceObjective
    ) -> MaintenancePlan:

        steps = [
            PlanStep(
                step_number=1,
                agent_name="MaintenanceAnalysisAgent",
                action="Analyze the maintenance issue and determine category, priority, specialization, and estimated duration"
            ),
            PlanStep(
                step_number=2,
                agent_name="TechnicianSchedulingAgent",
                action="Find a suitable available technician and conflict-free repair schedule"
            ),
            PlanStep(
                step_number=3,
                agent_name="ValidationSafetyAgent",
                action="Validate the recommendations using schema and deterministic business rules"
            ),
            PlanStep(
                step_number=4,
                agent_name="HumanApproval",
                action="Pause the workflow and request manager approval before assignment and scheduling"
            )
        ]

        return MaintenancePlan(steps=steps)