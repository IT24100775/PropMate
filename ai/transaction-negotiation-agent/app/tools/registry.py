import os
import httpx
from typing import Any, Callable
from pydantic import BaseModel, Field


BACKEND_URL = os.getenv(
    "BACKEND_INTERNAL_URL",
    "http://localhost:5235"
).rstrip("/")


class ToolInput(BaseModel):
    target_type: str
    target_id: int = Field(gt=0)
    objective: str = Field(min_length=1, max_length=1000)


class CounterOfferInput(BaseModel):
    target_type: str
    target_id: int = Field(gt=0)
    objective: str = Field(min_length=1, max_length=1000)
    proposal: dict[str, Any] = Field(default_factory=dict)


class ToolResult(BaseModel):
    tool_name: str
    succeeded: bool
    data: dict[str, Any]
    error: str | None = None


class ToolRegistry:
    def __init__(self):
        self._tools: dict[
            str,
            tuple[set[str], Callable[..., ToolResult]]
        ] = {}

    def register(
        self,
        name: str,
        allowed_agents: set[str],
        fn: Callable[..., ToolResult]
    ):
        self._tools[name] = (allowed_agents, fn)

    def execute(
        self,
        name: str,
        agent_role: str,
        payload: dict[str, Any],
        authorization: str | None = None
    ) -> ToolResult:

        if name not in self._tools:
            return ToolResult(
                tool_name=name,
                succeeded=False,
                data={},
                error="Tool is not allow-listed."
            )

        allowed_agents, fn = self._tools[name]

        if agent_role not in allowed_agents:
            return ToolResult(
                tool_name=name,
                succeeded=False,
                data={},
                error="Agent is not authorized for this tool."
            )

        try:
            # Normal tools use the standard ToolInput.
            if name != "execute_counter_offer":
                validated = ToolInput.model_validate(payload)
                return fn(validated, authorization)

            # Counter-offer execution requires the AI proposal as well.
            validated = CounterOfferInput.model_validate(payload)
            return fn(validated, authorization)

        except Exception as exc:
            return ToolResult(
                tool_name=name,
                succeeded=False,
                data={},
                error=(
                    "Tool input/output validation failed: "
                    f"{exc}"
                )
            )


# ============================================================
# TRANSACTION SNAPSHOT
# ============================================================

def get_transaction_snapshot(
    inp: ToolInput,
    authorization: str | None = None
) -> ToolResult:

    if not authorization:
        return ToolResult(
            tool_name="get_transaction_snapshot",
            succeeded=False,
            data={},
            error=(
                "Authorization token is required for "
                "transaction access."
            )
        )

    if inp.target_type in {
        "PurchaseOffer",
        "PurchaseNegotiation"
    }:
        endpoint = (
            f"{BACKEND_URL}/api/purchase-offers/"
            f"{inp.target_id}"
        )

    elif inp.target_type in {
        "RentalApplication",
        "RentalNegotiation"
    }:
        endpoint = (
            f"{BACKEND_URL}/api/rental-applications/"
            f"{inp.target_id}"
        )

    else:
        return ToolResult(
            tool_name="get_transaction_snapshot",
            succeeded=False,
            data={},
            error=(
                f"Unsupported target type: "
                f"{inp.target_type}"
            )
        )

    try:
        response = httpx.get(
            endpoint,
            headers={
                "Authorization": authorization
            },
            timeout=10.0
        )

        response.raise_for_status()

        return ToolResult(
            tool_name="get_transaction_snapshot",
            succeeded=True,
            data={
                "target_type": inp.target_type,
                "target_id": inp.target_id,
                "transaction": response.json()
            }
        )

    except httpx.HTTPStatusError as exc:
        return ToolResult(
            tool_name="get_transaction_snapshot",
            succeeded=False,
            data={},
            error=(
                "Transaction service returned HTTP "
                f"{exc.response.status_code}."
            )
        )

    except httpx.RequestError:
        return ToolResult(
            tool_name="get_transaction_snapshot",
            succeeded=False,
            data={},
            error=(
                "Transaction service is currently "
                "unavailable."
            )
        )


# ============================================================
# NEGOTIATION HISTORY
# ============================================================

def get_negotiation_history(
    inp: ToolInput,
    authorization: str | None = None
) -> ToolResult:

    if not authorization:
        return ToolResult(
            tool_name="get_negotiation_history",
            succeeded=False,
            data={},
            error=(
                "Authorization token is required for "
                "negotiation history access."
            )
        )

    if inp.target_type in {
        "PurchaseOffer",
        "PurchaseNegotiation"
    }:
        base_endpoint = (
            f"{BACKEND_URL}/api/purchase-offers/"
            f"{inp.target_id}/negotiation"
        )

    elif inp.target_type in {
        "RentalApplication",
        "RentalNegotiation"
    }:
        base_endpoint = (
            f"{BACKEND_URL}/api/rental-applications/"
            f"{inp.target_id}/negotiation"
        )

    else:
        return ToolResult(
            tool_name="get_negotiation_history",
            succeeded=False,
            data={},
            error=(
                f"Unsupported target type: "
                f"{inp.target_type}"
            )
        )

    headers = {
        "Authorization": authorization
    }

    try:
        with httpx.Client(timeout=10.0) as client:

            offers_response = client.get(
                f"{base_endpoint}/offers",
                headers=headers
            )

            offers_response.raise_for_status()

            messages_response = client.get(
                f"{base_endpoint}/messages",
                headers=headers
            )

            messages_response.raise_for_status()

        return ToolResult(
            tool_name="get_negotiation_history",
            succeeded=True,
            data={
                "target_type": inp.target_type,
                "target_id": inp.target_id,
                "offers": offers_response.json(),
                "messages": messages_response.json()
            }
        )

    except httpx.HTTPStatusError as exc:
        return ToolResult(
            tool_name="get_negotiation_history",
            succeeded=False,
            data={},
            error=(
                "Transaction service returned HTTP "
                f"{exc.response.status_code} while retrieving "
                "negotiation history."
            )
        )

    except httpx.RequestError:
        return ToolResult(
            tool_name="get_negotiation_history",
            succeeded=False,
            data={},
            error=(
                "Transaction service is currently "
                "unavailable."
            )
        )


# ============================================================
# PREPARE COUNTER OFFER
# ============================================================

def prepare_counter_offer(
    inp: ToolInput,
    authorization: str | None = None
) -> ToolResult:

    return ToolResult(
        tool_name="prepare_counter_offer",
        succeeded=True,
        data={
            "target_type": inp.target_type,
            "target_id": inp.target_id,
            "action": "PREPARE_COUNTER_OFFER",
            "requires_human_approval": True
        }
    )


# ============================================================
# EXECUTE COUNTER OFFER
# ============================================================

def execute_counter_offer(
    inp: CounterOfferInput,
    authorization: str | None = None
) -> ToolResult:

    tool_name = "execute_counter_offer"

    if not authorization:
        return ToolResult(
            tool_name=tool_name,
            succeeded=False,
            data={},
            error=(
                "Authorization token is required for "
                "counter-offer execution."
            )
        )

    # --------------------------------------------------------
    # Extract the AI-generated proposal.
    #
    # Gemini may return the actual action payload under
    # "action_payload", "actionPayload", or "payload".
    # --------------------------------------------------------

    proposal = inp.proposal or {}

    action_payload = (
        proposal.get("action_payload")
        or proposal.get("actionPayload")
        or proposal.get("payload")
        or proposal
    )

    if not isinstance(action_payload, dict):
        return ToolResult(
            tool_name=tool_name,
            succeeded=False,
            data={},
            error="AI proposal does not contain a valid action payload."
        )

    # --------------------------------------------------------
    # PURCHASE COUNTER OFFER
    # --------------------------------------------------------

    if inp.target_type in {
        "PurchaseOffer",
        "PurchaseNegotiation"
    }:

        offer_amount = (
            action_payload.get("offerAmount")
            or action_payload.get("OfferAmount")
            or action_payload.get("offer_amount")
        )

        conditions = (
            action_payload.get("conditions")
            or action_payload.get("Conditions")
        )

        if offer_amount is None:
            return ToolResult(
                tool_name=tool_name,
                succeeded=False,
                data={},
                error=(
                    "Purchase counter-offer is missing "
                    "OfferAmount."
                )
            )

        try:
            offer_amount = float(offer_amount)

            if offer_amount <= 0:
                raise ValueError

        except (TypeError, ValueError):
            return ToolResult(
                tool_name=tool_name,
                succeeded=False,
                data={},
                error=(
                    "Purchase counter-offer OfferAmount "
                    "must be a positive number."
                )
            )

        payload = {
            "offerAmount": offer_amount,
            "conditions": conditions
        }

        endpoint = (
            f"{BACKEND_URL}/api/purchase-offers/"
            f"{inp.target_id}/negotiation/counter"
        )

    # --------------------------------------------------------
    # RENTAL COUNTER OFFER
    # --------------------------------------------------------

    elif inp.target_type in {
        "RentalApplication",
        "RentalNegotiation"
    }:

        monthly_rent = (
            action_payload.get("monthlyRent")
            or action_payload.get("MonthlyRent")
            or action_payload.get("monthly_rent")
        )

        move_in_date = (
            action_payload.get("moveInDate")
            or action_payload.get("MoveInDate")
            or action_payload.get("move_in_date")
        )

        duration_months = (
            action_payload.get("durationMonths")
            or action_payload.get("DurationMonths")
            or action_payload.get("duration_months")
        )

        conditions = (
            action_payload.get("conditions")
            or action_payload.get("Conditions")
        )

        if monthly_rent is None:
            return ToolResult(
                tool_name=tool_name,
                succeeded=False,
                data={},
                error=(
                    "Rental counter-offer is missing "
                    "MonthlyRent."
                )
            )

        if move_in_date is None:
            return ToolResult(
                tool_name=tool_name,
                succeeded=False,
                data={},
                error=(
                    "Rental counter-offer is missing "
                    "MoveInDate."
                )
            )

        if duration_months is None:
            return ToolResult(
                tool_name=tool_name,
                succeeded=False,
                data={},
                error=(
                    "Rental counter-offer is missing "
                    "DurationMonths."
                )
            )

        try:
            monthly_rent = float(monthly_rent)

            if monthly_rent <= 0:
                raise ValueError

        except (TypeError, ValueError):
            return ToolResult(
                tool_name=tool_name,
                succeeded=False,
                data={},
                error=(
                    "Rental counter-offer MonthlyRent "
                    "must be a positive number."
                )
            )

        try:
            duration_months = int(duration_months)

            if duration_months < 1 or duration_months > 120:
                raise ValueError

        except (TypeError, ValueError):
            return ToolResult(
                tool_name=tool_name,
                succeeded=False,
                data={},
                error=(
                    "Rental counter-offer DurationMonths "
                    "must be between 1 and 120."
                )
            )

        payload = {
            "monthlyRent": monthly_rent,
            "moveInDate": str(move_in_date),
            "durationMonths": duration_months,
            "conditions": conditions
        }

        endpoint = (
            f"{BACKEND_URL}/api/rental-applications/"
            f"{inp.target_id}/negotiation/counter"
        )

    else:
        return ToolResult(
            tool_name=tool_name,
            succeeded=False,
            data={},
            error=(
                f"Unsupported target type: "
                f"{inp.target_type}"
            )
        )

    # --------------------------------------------------------
    # CALL ASP.NET BACKEND
    # --------------------------------------------------------

    try:
        response = httpx.post(
            endpoint,
            json=payload,
            headers={
                "Authorization": authorization,
                "Content-Type": "application/json"
            },
            timeout=10.0
        )

        response.raise_for_status()

        try:
            response_data = response.json()
        except ValueError:
            response_data = {
                "status_code": response.status_code,
                "text": response.text
            }

        return ToolResult(
            tool_name=tool_name,
            succeeded=True,
            data={
                "target_type": inp.target_type,
                "target_id": inp.target_id,
                "endpoint": endpoint,
                "submitted_payload": payload,
                "backend_response": response_data
            }
        )

    except httpx.HTTPStatusError as exc:

        try:
            error_body = exc.response.json()
        except ValueError:
            error_body = exc.response.text

        return ToolResult(
            tool_name=tool_name,
            succeeded=False,
            data={
                "target_type": inp.target_type,
                "target_id": inp.target_id
            },
            error=(
                "Counter-offer execution failed with HTTP "
                f"{exc.response.status_code}: {error_body}"
            )
        )

    except httpx.RequestError as exc:

        return ToolResult(
            tool_name=tool_name,
            succeeded=False,
            data={
                "target_type": inp.target_type,
                "target_id": inp.target_id
            },
            error=(
                "Transaction service is currently "
                f"unavailable: {exc}"
            )
        )


# ============================================================
# SAFE FAILURE
# ============================================================

def record_safe_failure(
    inp: ToolInput,
    authorization: str | None = None
) -> ToolResult:

    return ToolResult(
        tool_name="record_safe_failure",
        succeeded=True,
        data={
            "target_type": inp.target_type,
            "target_id": inp.target_id,
            "action": "SAFE_FAILURE_RECORDED"
        }
    )


# ============================================================
# BUILD TOOL REGISTRY
# ============================================================

def build_registry() -> ToolRegistry:

    registry = ToolRegistry()

    registry.register(
        "get_transaction_snapshot",
        {
            "DomainAnalysisAgent",
            "ActionAgent",
            "ValidationAgent"
        },
        get_transaction_snapshot
    )

    registry.register(
        "get_negotiation_history",
        {
            "DomainAnalysisAgent",
            "ActionAgent"
        },
        get_negotiation_history
    )

    registry.register(
        "prepare_counter_offer",
        {
            "ActionAgent"
        },
        prepare_counter_offer
    )

    registry.register(
        "execute_counter_offer",
        {
            "ActionAgent"
        },
        execute_counter_offer
    )

    registry.register(
        "record_safe_failure",
        {
            "ValidationAgent",
            "ActionAgent"
        },
        record_safe_failure
    )

    return registry