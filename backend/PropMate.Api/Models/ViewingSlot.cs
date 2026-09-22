using System.Text.Json.Serialization;

namespace PropMate.Api.Models;

public class ViewingSlot
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation property
    [JsonIgnore]
    public Property? Property { get; set; }
}
