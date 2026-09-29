from dataclasses import dataclass, field

from app.models.schemas import (
    PropertyListingInput,
    ToolEvidence,
)


@dataclass
class VerificationState:
    listing: PropertyListingInput

    evidence: list[ToolEvidence] = field(
        default_factory=list
    )

    risk_score: float = 0.0

    recommendation: str | None = None

    confidence: float = 0.0

    reasons: list[str] = field(
        default_factory=list
    )