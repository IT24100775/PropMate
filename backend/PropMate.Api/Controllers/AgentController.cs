using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PropMate.Api.DTOs.Agent;
using PropMate.Api.Services;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpPost("discover")]
    public async Task<ActionResult<AgentResponseDto>> Discover([FromBody] AgentQueryDto query)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
        {
            return BadRequest("Query is required.");
        }

        var result = await _agentService.DiscoverPropertiesAsync(query);
        return Ok(result);
    }
}
