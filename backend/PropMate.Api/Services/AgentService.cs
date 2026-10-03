using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PropMate.Api.DTOs.Agent;

namespace PropMate.Api.Services;

public class AgentService : IAgentService
{
    private readonly HttpClient _httpClient;

    public AgentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<AgentResponseDto> DiscoverPropertiesAsync(AgentQueryDto query)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/agent/discover", query);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<AgentResponseDto>();
            if (result == null) throw new Exception("Null response from AI Agent");
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            return new AgentResponseDto
            {
                Warnings = new System.Collections.Generic.List<string> { "Failed to communicate with AI Agent: " + ex.Message },
                Plan = new System.Collections.Generic.List<string> { "Error: Python Agent unavailable" }
            };
        }
    }
}
