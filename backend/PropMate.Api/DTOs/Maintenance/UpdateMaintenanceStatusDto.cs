namespace PropMate.Api.DTOs.Maintenance
{
    public class UpdateMaintenanceStatusDto
    {
        public string Status { get; set; } = string.Empty;

        public int ChangedBy { get; set; }

        public string? Comment { get; set; }
    }
}