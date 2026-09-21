namespace PropMate.Api.Models.Maintenance
{
    public class MaintenanceAssignment
    {
        public int Id { get; set; }

        public int MaintenanceRequestId { get; set; }

        public int TechnicianId { get; set; }

        public int AssignedBy { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }
    }
}