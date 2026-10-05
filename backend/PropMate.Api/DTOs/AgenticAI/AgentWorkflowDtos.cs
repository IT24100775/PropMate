using System.ComponentModel.DataAnnotations;
using PropMate.Api.Enums;
using System.Text.Json.Serialization;

namespace PropMate.Api.DTOs.AgenticAI;

public class StartAgentWorkflowDto
{
    [Required, MaxLength(1000)]
    public string Objective { get; set; } = string.Empty;

    [Required]
    public string TargetType { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TargetId { get; set; }
}

public class DecideAgentApprovalDto
{
    [Required]
    public AgentApprovalStatus Decision { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}

public class AgentWorkflowResponseDto
{
    public int Id { get; set; }

    [JsonPropertyName("workflow_id")]
    public Guid WorkflowId { get; set; }

    [JsonPropertyName("initiated_by_user_id")]
    public int InitiatedByUserId { get; set; }

    [JsonPropertyName("initiated_by_role")]
    public string InitiatedByRole { get; set; } = string.Empty;

    public string Objective { get; set; } = string.Empty;

    [JsonPropertyName("target_type")]
    public string TargetType { get; set; } = string.Empty;

    [JsonPropertyName("target_id")]
    public int TargetId { get; set; }

    public AgentWorkflowStatus Status { get; set; }

    [JsonPropertyName("plan")]
    public object? Plan { get; set; }

    [JsonPropertyName("final_outcome")]
    public object? FinalOutcome { get; set; }

    public string? Error { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    public List<AgentWorkflowStepDto> Steps { get; set; } = [];

    [JsonPropertyName("tool_calls")]
    public List<AgentToolCallDto> ToolCalls { get; set; } = [];

    public List<AgentApprovalDto> Approvals { get; set; } = [];
}

public class AgentWorkflowStepDto
{
    public int Sequence { get; set; }

    [JsonPropertyName("agent_role")]
    public string AgentRole { get; set; } = string.Empty;

    public string Responsibility { get; set; } = string.Empty;

    public AgentStepStatus Status { get; set; }

    [JsonPropertyName("input_json")]
    public object? Input { get; set; }

    [JsonPropertyName("output_json")]
    public object? Output { get; set; }

    [JsonPropertyName("validation_results")]
    public object? ValidationResults { get; set; }

    public string? Error { get; set; }

    [JsonPropertyName("retry_count")]
    public int RetryCount { get; set; }

    [JsonPropertyName("started_at")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }
}

public class AgentToolCallDto
{
    [JsonPropertyName("agent_role")]
    public string AgentRole { get; set; } = string.Empty;

    [JsonPropertyName("tool_name")]
    public string ToolName { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public object? Input { get; set; }

    [JsonPropertyName("output")]
    public object? Output { get; set; }

    public bool Succeeded { get; set; }

    public string? Error { get; set; }

    [JsonPropertyName("duration_ms")]
    public long DurationMs { get; set; }
}

public class AgentApprovalDto
{
    public int Id { get; set; }

    [JsonPropertyName("action_name")]
    public string ActionName { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public object? Payload { get; set; }

    public string Reason { get; set; } = string.Empty;

    public AgentApprovalStatus Status { get; set; }

    [JsonPropertyName("decided_by_user_id")]
    public int? DecidedByUserId { get; set; }

    [JsonPropertyName("decision_comment")]
    public string? DecisionComment { get; set; }

    [JsonPropertyName("requested_at")]
    public DateTime RequestedAt { get; set; }

    [JsonPropertyName("decided_at")]
    public DateTime? DecidedAt { get; set; }
}