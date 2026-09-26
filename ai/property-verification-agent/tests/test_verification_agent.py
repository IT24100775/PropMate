from unittest.mock import patch

from app.agent.verification_agent import (
    calculate_risk_score,
    run_verification,
)
from app.models.schemas import PropertyListingInput


# ---------------------------------------------------------
# Helper: valid low-risk listing
# ---------------------------------------------------------

def create_valid_listing():
    return PropertyListingInput(
        listing_id=1001,
        owner_id=1,
        title="Modern Three Bedroom House in Kandy",
        description=(
            "A spacious three bedroom house located in a "
            "residential area of Kandy."
        ),
        purpose="Sale",
        property_type="House",
        price=42000000,
        address="25 Peradeniya Road, Kandy",
        city="Kandy",
        bedrooms=3,
        bathrooms=2,

        images=[
            {
                "image_url": "https://example.com/property1.jpg",
                "is_primary": True,
            }
        ],

        owner_verified=True,
        duplicate_candidates=[],
        comparable_properties=[],
    )


# ---------------------------------------------------------
# Risk calculation tests
# ---------------------------------------------------------

def test_calculate_risk_score_no_risks():
    result = calculate_risk_score([])

    assert result == 0.0


def test_calculate_risk_score_low_risk():
    result = calculate_risk_score(
        [0.0, 0.0, 0.0, 0.2]
    )

    assert result == 0.15


def test_calculate_risk_score_high_risk():
    result = calculate_risk_score(
        [0.0, 0.7, 0.8, 0.9]
    )

    assert result == 0.81


# ---------------------------------------------------------
# Agent workflow tests
# ---------------------------------------------------------

def test_verification_executes_all_tools():
    listing = create_valid_listing()

    with patch(
        "app.agent.verification_agent.reason_about_listing",
        side_effect=RuntimeError(
            "Simulated AI failure"
        ),
    ):
        result = run_verification(listing)

    assert len(result.evidence) == 4

    tool_names = [
        item.tool_name
        for item in result.evidence
    ]

    assert "metadata_validator" in tool_names
    assert "owner_verifier" in tool_names
    assert "duplicate_checker" in tool_names
    assert "price_comparator" in tool_names


def test_valid_listing_has_low_risk():
    listing = create_valid_listing()

    with patch(
        "app.agent.verification_agent.reason_about_listing",
        side_effect=RuntimeError(
            "Simulated AI failure"
        ),
    ):
        result = run_verification(listing)

    assert result.risk_score == 0.15


def test_unverified_owner_increases_risk():
    listing = create_valid_listing()

    listing.owner_verified = False

    with patch(
        "app.agent.verification_agent.reason_about_listing",
        side_effect=RuntimeError(
            "Simulated AI failure"
        ),
    ):
        result = run_verification(listing)

    owner_evidence = next(
        item
        for item in result.evidence
        if item.tool_name == "owner_verifier"
    )

    assert owner_evidence.passed is False
    assert owner_evidence.risk_score == 0.7
    assert result.risk_score == 0.56


def test_ai_failure_falls_back_to_admin_review():
    listing = create_valid_listing()

    with patch(
        "app.agent.verification_agent.reason_about_listing",
        side_effect=RuntimeError(
            "Simulated Gemini service unavailable"
        ),
    ):
        result = run_verification(listing)

    assert result.recommendation == "ADMIN_REVIEW"
    assert result.confidence == 0.0

    assert (
        "Manual administrator review is required."
        in result.reasons[0]
    )