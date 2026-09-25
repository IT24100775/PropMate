namespace PropMate.Api.DTOs.Maintenance
{
    public class ApproveMaintenanceAiDto
    {
        public int ApprovedBy { get; set; }

        public int TechnicianId { get; set; }

        public DateTime ScheduledDate { get; set; }

        public string StartTime { get; set; } = "10:00";

        public string EndTime { get; set; } = "11:00";

        public string? Notes { get; set; }
    }
}