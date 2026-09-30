namespace PropMate.Api.DTOs.Maintenance
{
    public class CreateTechnicianDto
    {
        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public string AvailabilityStatus { get; set; } = "AVAILABLE";
    }
}