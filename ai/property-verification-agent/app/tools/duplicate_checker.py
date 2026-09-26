from app.models.schemas import (
    PropertyListingInput,
    ToolEvidence,
)


def check_duplicates(
    listing: PropertyListingInput,
) -> ToolEvidence:

    duplicates = [
        candidate
        for candidate in listing.duplicate_candidates
        if candidate.listing_id != listing.listing_id
    ]

    if duplicates:
        ids = ", ".join(
            str(candidate.listing_id)
            for candidate in duplicates
        )

        return ToolEvidence(
            tool_name="duplicate_checker",
            passed=False,
            risk_score=0.8,
            message=(
                f"Possible duplicate listings detected: {ids}."
            ),
        )

    return ToolEvidence(
        tool_name="duplicate_checker",
        passed=True,
        risk_score=0.0,
        message="No duplicate listings were detected.",
    )