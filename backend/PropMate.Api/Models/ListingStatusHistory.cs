using PropMate.Api.Enums;

namespace PropMate.Api.Models;

public class ListingStatusHistory
{
    public int Id { get; set; }

    public int PropertyListingId { get; set; }

    public ListingStatus PreviousStatus { get; set; }

    public ListingStatus NewStatus { get; set; }

    public string? Reason { get; set; }

    public int? ChangedByUserId { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public PropertyListing PropertyListing { get; set; } = null!;
}