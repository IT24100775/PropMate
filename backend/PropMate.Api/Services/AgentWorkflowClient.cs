using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PropMate.Api.DTOs.AgenticAI;
using PropMate.Api.Services.Interfaces;

namespace PropMate.Api.Services;

/// <summary>
/// Connects the ASP.NET Core backend to the separate Agentic AI service under the repository's /ai folder.
/// No Gemini SDK/API key is kept in this backend service.
/// </summary>
public class AgentWorkflowClient : IAgentWorkflowClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public AgentWorkflowClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AgentWorkflowResponseDto> StartAsync(
        int userId,
        string role,
        StartAgentWorkflowDto dto,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "workflows", userId, role);
        request.Content = JsonContent.Create(dto);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadRequiredAsync<AgentWorkflowResponseDto>(response, cancellationToken);
    }

    public async Task<AgentWorkflowResponseDto?> GetAsync(
        int workflowId,
        int userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"workflows/{workflowId}", userId, role);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        return await ReadRequiredAsync<AgentWorkflowResponseDto>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<AgentWorkflowResponseDto>> GetMineAsync(
        int userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "workflows", userId, role);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var items = await ReadRequiredAsync<List<AgentWorkflowResponseDto>>(response, cancellationToken);
        return items;
    }

    public async Task<AgentWorkflowResponseDto> DecideApprovalAsync(
        int workflowId,
        int approvalId,
        int userId,
        string role,
        DecideAgentApprovalDto dto,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(
            HttpMethod.Post,
            $"workflows/{workflowId}/approvals/{approvalId}/decision",
            userId,
            role);
        request.Content = JsonContent.Create(dto);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadRequiredAsync<AgentWorkflowResponseDto>(response, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl, int userId, string role)
    {
        var request = new HttpRequestMessage(method, relativeUrl);
        request.Headers.TryAddWithoutValidation("X-User-Id", userId.ToString());
        request.Headers.TryAddWithoutValidation("X-User-Role", role);

        var internalApiKey = _configuration["AgenticAiService:InternalApiKey"];
        if (!string.IsNullOrWhiteSpace(internalApiKey))
            request.Headers.TryAddWithoutValidation("X-Internal-Api-Key", internalApiKey);

        return request;
    }

    private async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var message = string.IsNullOrWhiteSpace(body)
                ? $"Agentic AI service returned HTTP {(int)response.StatusCode}."
                : $"Agentic AI service returned HTTP {(int)response.StatusCode}: {body}";
            throw new InvalidOperationException(message);
        }

        var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Agentic AI service returned an empty or invalid response.");
    }
}
