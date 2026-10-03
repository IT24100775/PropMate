namespace PropMate.Api.Models;

public class PropertyListingVerification
{
    public int Id { get; set; }

    public int PropertyListingId { get; set; }

    public string Recommendation { get; set; } = string.Empty;

    public double Confidence { get; set; }

    public double RiskScore { get; set; }

    public string ReasonsJson { get; set; } = "[]";

    public string EvidenceJson { get; set; } = "[]";

    public bool RequiresHumanReview { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PropertyListing PropertyListing { get; set; } = null!;
}