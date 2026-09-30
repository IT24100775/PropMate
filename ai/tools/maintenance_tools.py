import httpx
from pydantic import BaseModel


class MaintenanceRequestData(BaseModel):
    id: int
    property_id: int
    tenant_id: int
    description: str
    category: str
    priority: str
    status: str


class MaintenanceToolResult(BaseModel):
    success: bool
    data: object | None = None
    error: str | None = None


class MaintenanceTools:
    """
    Allow-listed tools for maintenance information.

    These tools are READ-ONLY.
    They communicate with the ASP.NET Core API.
    """

    BASE_URL = "http://localhost:5235/api"

    ALLOWED_TOOLS = {
        "GetMaintenanceRequest",
        "GetMaintenanceHistory"
    }

    def get_maintenance_request(
        self,
        request_id: int
    ) -> MaintenanceToolResult:

        # Input validation
        if request_id <= 0:
            return MaintenanceToolResult(
                success=False,
                error="Invalid maintenance request ID."
            )

        try:
            response = httpx.get(
                f"{self.BASE_URL}/Maintenance/{request_id}",
                timeout=10.0
            )

            if response.status_code == 404:
                return MaintenanceToolResult(
                    success=False,
                    error="Maintenance request not found."
                )

            response.raise_for_status()

            data = response.json()

            return MaintenanceToolResult(
                success=True,
                data=data
            )

        except httpx.HTTPError as e:
            return MaintenanceToolResult(
                success=False,
                error=f"Maintenance API error: {str(e)}"
            )

    def get_maintenance_history(
        self,
        request_id: int
    ) -> MaintenanceToolResult:

        # Input validation
        if request_id <= 0:
            return MaintenanceToolResult(
                success=False,
                error="Invalid maintenance request ID."
            )

        try:
            response = httpx.get(
                f"{self.BASE_URL}/Maintenance/{request_id}/history",
                timeout=10.0
            )

            if response.status_code == 404:
                return MaintenanceToolResult(
                    success=False,
                    error="Maintenance request not found."
                )

            response.raise_for_status()

            return MaintenanceToolResult(
                success=True,
                data=response.json()
            )

        except httpx.HTTPError as e:
            return MaintenanceToolResult(
                success=False,
                error=f"Maintenance history API error: {str(e)}"
            )