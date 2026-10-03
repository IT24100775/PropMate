namespace PropMate.Api.Models.Maintenance
{
    public class MaintenanceStatusHistory
    {
        public int Id { get; set; }

        public int MaintenanceRequestId { get; set; }

        public string OldStatus { get; set; } = string.Empty;

        public string NewStatus { get; set; } = string.Empty;

        public int ChangedBy { get; set; }

        public string? Comment { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}