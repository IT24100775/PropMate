namespace PropMate.Api.DTOs.Maintenance
{
    public class MaintenanceAiRequestDto
    {
        public int MaintenanceRequestId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }
}