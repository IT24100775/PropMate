namespace PropMate.Api.Models.Maintenance
{
    public class MaintenanceNotification
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public int MaintenanceRequestId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}