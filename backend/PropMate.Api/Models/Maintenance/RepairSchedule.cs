namespace PropMate.Api.Models.Maintenance
{
    public class RepairSchedule
    {
        public int Id { get; set; }

        public int MaintenanceRequestId { get; set; }

        public int TechnicianId { get; set; }

        public DateTime ScheduledDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}