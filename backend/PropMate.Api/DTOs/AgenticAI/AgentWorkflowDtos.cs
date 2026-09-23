using System.ComponentModel.DataAnnotations;
using PropMate.Api.Enums;

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
    public Guid WorkflowId { get; set; }
    public int InitiatedByUserId { get; set; }
    public string Objective { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public AgentWorkflowStatus Status { get; set; }
    public string? PlanJson { get; set; }
    public string? FinalOutcomeJson { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<AgentWorkflowStepDto> Steps { get; set; } = [];
    public List<AgentToolCallDto> ToolCalls { get; set; } = [];
    public List<AgentApprovalDto> Approvals { get; set; } = [];
}

public class AgentWorkflowStepDto
{
    public int Sequence { get; set; }
    public string AgentRole { get; set; } = string.Empty;
    public string Responsibility { get; set; } = string.Empty;
    public AgentStepStatus Status { get; set; }
    public string InputJson { get; set; } = "{}";
    public string? OutputJson { get; set; }
    public string? ValidationResultJson { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class AgentToolCallDto
{
    public string AgentRole { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string InputJson { get; set; } = "{}";
    public string? OutputJson { get; set; }
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public long DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AgentApprovalDto
{
    public int Id { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public string Reason { get; set; } = string.Empty;
    public AgentApprovalStatus Status { get; set; }
    public int? DecidedByUserId { get; set; }
    public string? DecisionComment { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
}
