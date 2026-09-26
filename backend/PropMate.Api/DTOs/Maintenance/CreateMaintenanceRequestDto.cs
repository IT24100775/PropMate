namespace PropMate.Api.DTOs.Maintenance
{
    public class CreateTenantMaintenanceRequestDto
    {
        public int PropertyListingId { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Priority { get; set; } = "MEDIUM";
    }

    public class CreateMaintenanceRequestDto
    {
        public int PropertyId { get; set; }

        public int TenantId { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Priority { get; set; } = "MEDIUM";

        public string? ImageUrl { get; set; }
    }
}