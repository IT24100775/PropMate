using Google.GenAI;
using PropMate.Api.DTOs.Maintenance;
using PropMate.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace PropMate.Api.Services.Maintenance
{
    public class MaintenanceAiService : IMaintenanceAiService
    {
        private readonly Client _client;
        private readonly ApplicationDbContext _context;

        public MaintenanceAiService(IConfiguration configuration, ApplicationDbContext context)
        {
            var apiKey = configuration["GEMINI_API_KEY"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Gemini API key is not configured.");
            }

            _client = new Client(apiKey: apiKey);
            _context = context;
        }

        public async Task<MaintenanceAiResponseDto> AnalyzeAsync(
            MaintenanceAiRequestDto request)
        {
            var prompt = $"""
    You are a maintenance coordination assistant
    for a property rental management system.

    Analyze this maintenance issue:

    Description: {request.Description}
    Current Category: {request.Category}
    Current Priority: {request.Priority}

    Recommend:
    1. Correct maintenance category
    2. Correct priority
    3. Suitable technician specialization
    4. Practical repair schedule
    5. Short explanation

    Return only JSON with these fields:
    recommendedCategory
    recommendedPriority
    recommendedTechnicianSpecialization
    recommendedSchedule
    recommendation
    """;

            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-3.6-flash",
                contents: prompt
            );

            var json = response.Text ?? "{}";

            var result =
                System.Text.Json.JsonSerializer.Deserialize<MaintenanceAiResponseDto>(
                    json,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
{
    throw new Exception("Gemini returned an invalid response.");
}

var recommendedSpecialization =
    result.RecommendedTechnicianSpecialization.ToLower();

var matchingTechnicians = await _context.Technicians
    .Where(t =>
        t.Specialization.ToLower().Contains(recommendedSpecialization)
        && t.AvailabilityStatus.ToUpper() == "AVAILABLE")
    .ToListAsync();

result.MatchingTechnicians = matchingTechnicians;

var maintenanceRequest = await _context.MaintenanceRequests
    .FirstOrDefaultAsync(x => x.Id == request.MaintenanceRequestId);

if (maintenanceRequest == null)
{
    throw new Exception("Maintenance request not found.");
}

maintenanceRequest.AiCategory = result.RecommendedCategory;
maintenanceRequest.AiPriority = result.RecommendedPriority;
maintenanceRequest.AiTechnicianSpecialization =
    result.RecommendedTechnicianSpecialization;
maintenanceRequest.AiRecommendedSchedule =
    result.RecommendedSchedule;
maintenanceRequest.AiRecommendation =
    result.Recommendation;

maintenanceRequest.UpdatedAt = DateTime.UtcNow;

await _context.SaveChangesAsync();

return result;
        }
    }
}