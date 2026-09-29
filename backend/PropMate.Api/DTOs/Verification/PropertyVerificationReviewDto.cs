namespace PropMate.Api.DTOs.Verification;

public class PropertyVerificationReviewDto
{
    // Listing information
    public int ListingId { get; set; }

    public int OwnerId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public int Bedrooms { get; set; }

    public int Bathrooms { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<string> ImageUrls { get; set; } = [];

    // AI verification information
    public string Recommendation { get; set; } = string.Empty;

    public double Confidence { get; set; }

    public double RiskScore { get; set; }

    public List<string> Reasons { get; set; } = [];

    public List<VerificationEvidenceResponse> Evidence { get; set; } = [];

    public bool RequiresHumanReview { get; set; }

    public DateTime VerifiedAt { get; set; }
}