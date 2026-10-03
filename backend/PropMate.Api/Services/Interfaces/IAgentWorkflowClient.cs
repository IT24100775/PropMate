using PropMate.Api.DTOs.AgenticAI;

namespace PropMate.Api.Services.Interfaces;

/// <summary>
/// HTTP client contract for the Agentic AI service that lives outside the backend folder.
/// The AI service owns the four-agent orchestration (Planner, Domain Analysis, Action, Validation),
/// while ASP.NET Core remains the authenticated gateway for the web and mobile clients.
/// </summary>
public interface IAgentWorkflowClient
{
    Task<AgentWorkflowResponseDto> StartAsync(int userId, string role, StartAgentWorkflowDto dto, CancellationToken cancellationToken = default);
    Task<AgentWorkflowResponseDto?> GetAsync(int workflowId, int userId, string role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgentWorkflowResponseDto>> GetMineAsync(int userId, string role, CancellationToken cancellationToken = default);
    Task<AgentWorkflowResponseDto> DecideApprovalAsync(int workflowId, int approvalId, int userId, string role, DecideAgentApprovalDto dto, CancellationToken cancellationToken = default);
}
