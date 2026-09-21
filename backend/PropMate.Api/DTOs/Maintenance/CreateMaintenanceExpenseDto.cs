namespace PropMate.Api.DTOs.Maintenance
{
    public class CreateMaintenanceExpenseDto
    {
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public int RecordedBy { get; set; }
    }
}