using System;

namespace PropMate.Api.DTOs.Viewing
{
    public class CreateViewingSlotDto
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
