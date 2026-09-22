using System.Text.Json.Serialization;

namespace PropMate.Api.Models;

public class ViewingBooking
{
    public int Id { get; set; }
    public int ViewingSlotId { get; set; }
    public int UserId { get; set; }
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [JsonIgnore]
    public ViewingSlot? ViewingSlot { get; set; }
}
