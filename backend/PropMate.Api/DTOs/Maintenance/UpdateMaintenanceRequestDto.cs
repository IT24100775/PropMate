namespace PropMate.Api.DTOs.Maintenance
{
    public class UpdateMaintenanceRequestDto
    {
        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Priority { get; set; } = "MEDIUM";

        public string? ImageUrl { get; set; }
    }
}