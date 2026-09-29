using System;
using PropMate.Api.Enums;

namespace PropMate.Api.DTOs.Viewing
{
    public class ViewingBookingDto
    {
        public int Id { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime BookedAt { get; set; }
        
        public int ViewingSlotId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int PropertyListingId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public string PropertyCity { get; set; } = string.Empty;
        public string? PrimaryImageUrl { get; set; }
    }
}
