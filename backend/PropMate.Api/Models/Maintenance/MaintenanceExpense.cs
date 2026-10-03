namespace PropMate.Api.Models.Maintenance
{
    public class MaintenanceExpense
    {
        public int Id { get; set; }

        public int MaintenanceRequestId { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public int RecordedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}