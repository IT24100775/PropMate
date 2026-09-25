namespace PropMate.Api.DTOs.Maintenance
{
    public class MaintenanceQueryDto
    {
        public string? Search { get; set; }

        public string? Status { get; set; }

        public string? Priority { get; set; }

        public string? Category { get; set; }

        public int? PropertyId { get; set; }

        public int? TenantId { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string SortBy { get; set; } = "CreatedAt";

        public string SortOrder { get; set; } = "desc";
    }
}