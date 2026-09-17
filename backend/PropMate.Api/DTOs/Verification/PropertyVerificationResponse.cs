using System.Text.Json.Serialization;

namespace PropMate.Api.DTOs.Verification;

public class PropertyVerificationResponse
{
    [JsonPropertyName("listing_id")]
    public int ListingId { get; set; }

    [JsonPropertyName("recommendation")]
    public string Recommendation { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("risk_score")]
    public double RiskScore { get; set; }

    [JsonPropertyName("reasons")]
    public List<string> Reasons { get; set; } = [];

    [JsonPropertyName("evidence")]
    public List<VerificationEvidenceResponse> Evidence { get; set; } = [];
}

public class VerificationEvidenceResponse
{
    [JsonPropertyName("tool_name")]
    public string ToolName { get; set; } = string.Empty;

    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    [JsonPropertyName("risk_score")]
    public double RiskScore { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}