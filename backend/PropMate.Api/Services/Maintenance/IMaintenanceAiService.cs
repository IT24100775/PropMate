using PropMate.Api.DTOs.Maintenance;

namespace PropMate.Api.Services.Maintenance
{
    public interface IMaintenanceAiService
    {
        Task<MaintenanceAiResponseDto> AnalyzeAsync(
            MaintenanceAiRequestDto request);
    }
}