from google.genai.errors import APIError

from app.agent.state import VerificationState
from app.agent.gemini_reasoner import reason_about_listing
from app.models.schemas import PropertyListingInput

from app.tools.metadata_validator import validate_metadata
from app.tools.owner_verifier import verify_owner
from app.tools.duplicate_checker import check_duplicates
from app.tools.price_comparator import compare_market_price

def calculate_risk_score(
    risk_scores: list[float],
) -> float:

    if not risk_scores:
        return 0.0

    average_risk = sum(risk_scores) / len(risk_scores)
    maximum_risk = max(risk_scores)

    combined_risk = (
        0.7 * maximum_risk
        + 0.3 * average_risk
    )

    return round(combined_risk, 2)

def run_verification(
    listing: PropertyListingInput,
) -> VerificationState:

    state = VerificationState(listing=listing)

    # Deterministic verification tools
    tools = [
        validate_metadata,
        verify_owner,
        check_duplicates,
        compare_market_price,
    ]

    # Execute each controlled tool
    for tool in tools:
        evidence = tool(listing)
        state.evidence.append(evidence)

    # Calculate overall deterministic risk score
    if state.evidence:
        state.risk_score = calculate_risk_score(
            [
                item.risk_score
                for item in state.evidence
            ]
        )

    # Send the collected evidence to Gemini for reasoning
    try:
        decision = reason_about_listing(
            listing=listing,
            evidence=state.evidence,
        )

        state.recommendation = decision.recommendation.value
        state.confidence = decision.confidence
        state.reasons = decision.reasons

    # Gemini/API unavailable
    except APIError as error:
        print(
            f"Gemini API error during verification: {error}"
        )

        state.recommendation = "ADMIN_REVIEW"
        state.confidence = 0.0
        state.reasons = [
            "AI reasoning service is temporarily unavailable. "
            "Manual administrator review is required."
        ]

    # Any other unexpected AI failure
    except Exception as error:
        print(
            f"Unexpected AI verification error: {error}"
        )

        state.recommendation = "ADMIN_REVIEW"
        state.confidence = 0.0
        state.reasons = [
            "AI verification could not be completed. "
            "Manual administrator review is required."
        ]

    return state