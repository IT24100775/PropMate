using System.ComponentModel.DataAnnotations;
using PropMate.Api.Enums;

namespace PropMate.Api.DTOs.Listings;

public class PropertyListingQueryDto
{
    public string? Search { get; set; }

    public string? City { get; set; }

    public ListingPurpose? Purpose { get; set; }

    public PropertyType? PropertyType { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; set; }

    public string SortBy { get; set; } = "createdAt";

    public string SortOrder { get; set; } = "desc";

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}