using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropMate.Api.DTOs.AgenticAI;
using PropMate.Api.Services.Interfaces;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/ai-workflows")]
[Authorize(Roles = "OwnerAgent,Admin")]
public class AiWorkflowsController : ControllerBase
{
    private readonly IAgentWorkflowClient _client;

    public AiWorkflowsController(IAgentWorkflowClient client)
    {
        _client = client;
    }

    [HttpPost]
    [Authorize(Roles = "OwnerAgent")]
    public async Task<ActionResult<AgentWorkflowResponseDto>> Start(
        [FromBody] StartAgentWorkflowDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _client.StartAsync(UserId(), Role(), dto, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgentWorkflowResponseDto>>> List(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _client.GetMineAsync(UserId(), Role(), cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AgentWorkflowResponseDto>> Get(int id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _client.GetAsync(id, UserId(), Role(), cancellationToken);
            return item == null ? NotFound() : Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/approvals/{approvalId:int}/decision")]
    [Authorize(Roles = "OwnerAgent")]
    public async Task<ActionResult<AgentWorkflowResponseDto>> Decide(
        int id,
        int approvalId,
        [FromBody] DecideAgentApprovalDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _client.DecideApprovalAsync(id, approvalId, UserId(), Role(), dto, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }

    private int UserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string Role() => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}
