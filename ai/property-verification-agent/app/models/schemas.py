from enum import Enum
from typing import List
from pydantic import BaseModel, Field


class ListingPurpose(str, Enum):
    SALE = "Sale"
    RENT = "Rent"


class PropertyType(str, Enum):
    HOUSE = "House"
    APARTMENT = "Apartment"
    LAND = "Land"
    COMMERCIAL = "Commercial"


class PropertyImageInput(BaseModel):
    image_url: str
    is_primary: bool = False


class DuplicateCandidate(BaseModel):
    listing_id: int
    title: str
    address: str

class ComparableProperty(BaseModel):
    listing_id: int
    price: float = Field(gt=0)
    city: str
    property_type: PropertyType

class PropertyListingInput(BaseModel):
    listing_id: int
    owner_id: int

    title: str
    description: str

    purpose: ListingPurpose
    property_type: PropertyType

    price: float = Field(gt=0)

    address: str
    city: str

    bedrooms: int = Field(ge=0)
    bathrooms: int = Field(ge=0)

    images: List[PropertyImageInput] = Field(default_factory=list)
    owner_verified: bool = False

    duplicate_candidates: List[DuplicateCandidate] = Field(
        default_factory=list
    )

    comparable_properties: List[ComparableProperty] = Field(
    default_factory=list
    )

class ToolEvidence(BaseModel):
    tool_name: str
    passed: bool
    risk_score: float = Field(ge=0, le=1)
    message: str

class VerificationRecommendation(str, Enum):
    APPROVE = "APPROVE"
    REQUEST_INFORMATION = "REQUEST_INFORMATION"
    ADMIN_REVIEW = "ADMIN_REVIEW"
    REJECT = "REJECT"

class AgentDecision(BaseModel):
    recommendation: VerificationRecommendation
    confidence: float = Field(ge=0, le=1)
    reasons: List[str]