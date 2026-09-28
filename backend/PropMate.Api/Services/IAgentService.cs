using System.Threading.Tasks;
using PropMate.Api.DTOs.Agent;

namespace PropMate.Api.Services;

public interface IAgentService
{
    Task<AgentResponseDto> DiscoverPropertiesAsync(AgentQueryDto query);
}
