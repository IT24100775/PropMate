using PropMate.Api.Models.Maintenance;

namespace PropMate.Api.DTOs.Maintenance
{
    public class MaintenanceAiResponseDto
    {
        public string RecommendedCategory { get; set; } = string.Empty;
        public string RecommendedPriority { get; set; } = string.Empty;
        public string RecommendedTechnicianSpecialization { get; set; } = string.Empty;
        public string RecommendedSchedule { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;

        public List<Technician> MatchingTechnicians { get; set; } = new();
    }
}