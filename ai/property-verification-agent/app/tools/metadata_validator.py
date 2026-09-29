from app.models.schemas import (
    PropertyListingInput,
    ToolEvidence,
)


def validate_metadata(
    listing: PropertyListingInput,
) -> ToolEvidence:

    issues: list[str] = []

    if len(listing.title.strip()) < 5:
        issues.append("Title is too short.")

    if len(listing.description.strip()) < 20:
        issues.append("Description is too short.")

    if not listing.address.strip():
        issues.append("Address is missing.")

    if not listing.city.strip():
        issues.append("City is missing.")

    if not listing.images:
        issues.append("At least one image is required.")

    if issues:
        return ToolEvidence(
            tool_name="metadata_validator",
            passed=False,
            risk_score=0.8,
            message=" ".join(issues),
        )

    return ToolEvidence(
        tool_name="metadata_validator",
        passed=True,
        risk_score=0.0,
        message="Required listing metadata is complete.",
    )