using System;

namespace PropMate.Api.DTOs.Viewing
{
    public class ViewingSlotDto
    {
        public int Id { get; set; }
        public int PropertyListingId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}
