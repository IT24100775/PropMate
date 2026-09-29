using System.Text.Json.Serialization;

namespace PropMate.Api.Models;

public class Favourite
{
    public int Id { get; set; }

    public int PropertyListingId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public PropertyListing? PropertyListing { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}