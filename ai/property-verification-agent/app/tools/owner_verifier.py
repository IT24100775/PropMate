from app.models.schemas import (
    PropertyListingInput,
    ToolEvidence,
)


def verify_owner(
    listing: PropertyListingInput,
) -> ToolEvidence:

    if listing.owner_verified:
        return ToolEvidence(
            tool_name="owner_verifier",
            passed=True,
            risk_score=0.0,
            message="Property owner is verified.",
        )

    return ToolEvidence(
        tool_name="owner_verifier",
        passed=False,
        risk_score=0.7,
        message="Property owner has not been verified.",
    )