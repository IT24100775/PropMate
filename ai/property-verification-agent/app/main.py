from fastapi import FastAPI

from app.agent.verification_agent import run_verification
from app.models.schemas import PropertyListingInput


app = FastAPI(
    title="PropMate Property Verification Agent",
    version="1.0.0",
)


@app.get("/health")
def health_check():
    return {
        "status": "healthy",
        "service": "property-verification-agent",
    }


@app.post("/verify-listing")
def verify_listing(listing: PropertyListingInput):
    state = run_verification(listing)

    return {
        "listing_id": listing.listing_id,
        "recommendation": state.recommendation,
        "confidence": state.confidence,
        "risk_score": state.risk_score,
        "reasons": state.reasons,
        "evidence": [
            evidence.model_dump()
            for evidence in state.evidence
        ],
    }