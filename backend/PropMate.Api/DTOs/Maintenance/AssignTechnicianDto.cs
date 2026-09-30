namespace PropMate.Api.DTOs.Maintenance
{
    public class AssignTechnicianDto
    {
        public int TechnicianId { get; set; }

        public int AssignedBy { get; set; }

        public string? Notes { get; set; }
    }
}