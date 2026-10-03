using System.Collections.Generic;
using System.Text.Json;

namespace PropMate.Api.DTOs.Agent;

public class AgentResponseDto
{
    public Dictionary<string, System.Text.Json.JsonElement> InterpretedCriteria { get; set; } = new();
    public List<string> Plan { get; set; } = new();
    public List<AgentMatchDto> Matches { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public float Confidence { get; set; } = 0.0f;
}

public class AgentMatchDto
{
    public int PropertyListingId { get; set; }
    public string Reasons { get; set; } = string.Empty;
    public List<AgentViewingSlotDto> AvailableViewingSlots { get; set; } = new();
}

public class AgentViewingSlotDto
{
    public int Id { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}
