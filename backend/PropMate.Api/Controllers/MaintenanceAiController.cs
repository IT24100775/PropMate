using Microsoft.AspNetCore.Mvc;
using PropMate.Api.DTOs.Maintenance;
using PropMate.Api.Services.Maintenance;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;

namespace PropMate.Api.Controllers
{
    [ApiController]
    [Route("api/maintenance-ai")]
    public class MaintenanceAiController : ControllerBase
    {
        private readonly IMaintenanceAiService _aiService;

        private readonly ApplicationDbContext _context;

        public MaintenanceAiController(IMaintenanceAiService aiService, ApplicationDbContext context)
        {
            _aiService = aiService;
            _context = context;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<MaintenanceAiResponseDto>> Analyze(
            MaintenanceAiRequestDto request)
        {
            var result = await _aiService.AnalyzeAsync(request);

            return Ok(result);
        }

        [HttpPost("{id}/approve")]
public async Task<IActionResult> ApproveAiRecommendation(
    int id,
    ApproveMaintenanceAiDto request)
{
    var maintenanceRequest =
        await _context.MaintenanceRequests
            .FirstOrDefaultAsync(x => x.Id == id);

    if (maintenanceRequest == null)
    {
        return NotFound("Maintenance request not found.");
    }

    if (string.IsNullOrEmpty(maintenanceRequest.AiRecommendation))
    {
        return BadRequest("No AI recommendation is available to approve.");
    }

    maintenanceRequest.AiApproved = true;
    maintenanceRequest.AiApprovedBy = request.ApprovedBy;
    maintenanceRequest.AiApprovedAt = DateTime.UtcNow;
    maintenanceRequest.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "AI recommendation approved successfully.",
        maintenanceRequestId = id,
        approvedBy = request.ApprovedBy,
        approvedAt = maintenanceRequest.AiApprovedAt
    });
}
    }
}