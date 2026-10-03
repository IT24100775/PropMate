namespace PropMate.Api.DTOs.Maintenance
{
    public class TechnicianQueryDto
    {
        public string? Search { get; set; }

        public string? Specialization { get; set; }

        public string? AvailabilityStatus { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string SortBy { get; set; } = "Name";

        public string SortOrder { get; set; } = "asc";
    }
}