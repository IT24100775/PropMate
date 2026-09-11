using System.ComponentModel.DataAnnotations;
using PropMate.Api.Enums;

namespace PropMate.Api.DTOs.Listings;

public class UpdatePropertyListingDto
{
    [Required]
    [StringLength(150, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public ListingPurpose Purpose { get; set; }

    [Required]
    public PropertyType PropertyType { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Range(0, 100)]
    public int Bedrooms { get; set; }

    [Range(0, 100)]
    public int Bathrooms { get; set; }

    public List<string> ImageUrls { get; set; } = new();
}