from app.models.schemas import (
    PropertyListingInput,
    ToolEvidence,
)


def compare_market_price(
    listing: PropertyListingInput,
) -> ToolEvidence:

    comparables = listing.comparable_properties

    if not comparables:
        return ToolEvidence(
            tool_name="price_comparator",
            passed=True,
            risk_score=0.2,
            message=(
                "No comparable properties were available. "
                "Price could not be fully verified."
            ),
        )

    average_price = sum(
        item.price for item in comparables
    ) / len(comparables)

    difference = abs(
        listing.price - average_price
    ) / average_price

    difference_percentage = round(
        difference * 100,
        2,
    )

    if difference > 0.50:
        return ToolEvidence(
            tool_name="price_comparator",
            passed=False,
            risk_score=0.9,
            message=(
                f"Listing price differs from comparable "
                f"properties by {difference_percentage}%."
            ),
        )

    if difference > 0.25:
        return ToolEvidence(
            tool_name="price_comparator",
            passed=False,
            risk_score=0.6,
            message=(
                f"Listing price differs from comparable "
                f"properties by {difference_percentage}%."
            ),
        )

    return ToolEvidence(
        tool_name="price_comparator",
        passed=True,
        risk_score=0.1,
        message=(
            f"Listing price is reasonably close to the "
            f"average comparable price of "
            f"{average_price:.2f}."
        ),
    )