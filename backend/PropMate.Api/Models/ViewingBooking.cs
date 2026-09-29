using System.Text.Json.Serialization;
using PropMate.Api.Enums;

namespace PropMate.Api.Models;

public class ViewingBooking
{
    public int Id { get; set; }

    public int ViewingSlotId { get; set; }

    public int UserId { get; set; }

    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    public BookingStatus Status { get; set; } = BookingStatus.Booked;

    [JsonIgnore]
    public ViewingSlot? ViewingSlot { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}