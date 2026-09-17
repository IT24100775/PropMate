using System.Text.Json.Serialization;

namespace PropMate.Api.DTOs.Verification;

public class PropertyVerificationRequest
{
    [JsonPropertyName("listing_id")]
    public int ListingId { get; set; }

    [JsonPropertyName("owner_id")]
    public int OwnerId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [JsonPropertyName("property_type")]
    public string PropertyType { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("bedrooms")]
    public int Bedrooms { get; set; }

    [JsonPropertyName("bathrooms")]
    public int Bathrooms { get; set; }

    [JsonPropertyName("images")]
    public List<VerificationImageRequest> Images { get; set; } = [];

    [JsonPropertyName("owner_verified")]
    public bool OwnerVerified { get; set; }

    [JsonPropertyName("duplicate_candidates")]
    public List<DuplicateCandidateRequest> DuplicateCandidates { get; set; } = [];

    [JsonPropertyName("comparable_properties")]
    public List<ComparablePropertyRequest> ComparableProperties { get; set; } = [];
}

public class VerificationImageRequest
{
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("is_primary")]
    public bool IsPrimary { get; set; }
}

public class DuplicateCandidateRequest
{
    [JsonPropertyName("listing_id")]
    public int ListingId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;
}

public class ComparablePropertyRequest
{
    [JsonPropertyName("listing_id")]
    public int ListingId { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("property_type")]
    public string PropertyType { get; set; } = string.Empty;
}