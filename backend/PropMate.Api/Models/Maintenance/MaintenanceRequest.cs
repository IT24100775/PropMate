namespace PropMate.Api.Models.Maintenance
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }

        public int TenantId { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Priority { get; set; } = "MEDIUM";

        public string Status { get; set; } = "PENDING";

        public string? ImageUrl { get; set; }

        public string? AiCategory { get; set; }

        public string? AiPriority { get; set; }

        public string? AiTechnicianSpecialization { get; set; }
        public string? AiRecommendedSchedule { get; set; }   

        public string? AiRecommendation { get; set; }

        public bool ApprovalRequired { get; set; } = false;

        public int? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public bool AiApproved { get; set; } = false;

        public int? AiApprovedBy { get; set; }

        public DateTime? AiApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }
    }
}