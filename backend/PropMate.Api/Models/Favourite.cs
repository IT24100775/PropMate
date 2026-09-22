using System.Text.Json.Serialization;

namespace PropMate.Api.Models;

public class Favourite
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [JsonIgnore]
    public Property? Property { get; set; }
}
