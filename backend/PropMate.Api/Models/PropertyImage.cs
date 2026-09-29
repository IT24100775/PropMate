namespace PropMate.Api.Models;

public class PropertyImage
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int PropertyListingId { get; set; }

    public PropertyListing PropertyListing { get; set; } = null!;
}