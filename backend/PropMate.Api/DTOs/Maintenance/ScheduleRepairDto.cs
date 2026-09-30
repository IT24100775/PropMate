namespace PropMate.Api.DTOs.Maintenance
{
    public class ScheduleRepairDto
    {
        public int TechnicianId { get; set; }

        public DateTime ScheduledDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string? Notes { get; set; }
    }
}