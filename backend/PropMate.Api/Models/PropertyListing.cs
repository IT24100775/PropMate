using PropMate.Api.Enums;

namespace PropMate.Api.Models;

public class PropertyListing
{
    public int Id { get; set; }

    public int OwnerId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ListingPurpose Purpose { get; set; }

    public PropertyType PropertyType { get; set; }

    public decimal Price { get; set; }

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public int Bedrooms { get; set; }

    public int Bathrooms { get; set; }

    public ListingStatus Status { get; set; } = ListingStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PropertyImage> Images { get; set; }
        = new List<PropertyImage>();

    public ICollection<ListingStatusHistory> StatusHistory { get; set; }
        = new List<ListingStatusHistory>();
}