using PropMate.Api.DTOs.Maintenance;
using PropMate.Api.Models.Maintenance;

namespace PropMate.Api.Services.Maintenance
{
    public interface IMaintenanceService
    {
        Task<MaintenanceRequest> CreateAsync(
            CreateMaintenanceRequestDto dto);

        Task<(List<MaintenanceRequest> Items, int TotalCount)>
            GetAllAsync(MaintenanceQueryDto query);

        Task<List<MaintenanceNotification>> GetNotificationsForTenantAsync(int tenantId);

        Task<MaintenanceRequest?> GetByIdAsync(int id);

        Task<MaintenanceRequest?> UpdateAsync(
            int id,
            UpdateMaintenanceRequestDto dto);

        Task<bool> DeleteAsync(int id);

        Task<MaintenanceAssignment?> AssignTechnicianAsync(
            int maintenanceRequestId,
            AssignTechnicianDto dto);

        Task<object?> ApproveAiRecommendationAsync(
            int maintenanceRequestId,
            ApproveMaintenanceAiDto dto);

        Task<RepairSchedule?> ScheduleRepairAsync(
    int maintenanceRequestId,
    ScheduleRepairDto dto);

    //Status workflow
    Task<MaintenanceRequest?> UpdateStatusAsync(
    int maintenanceRequestId,
    UpdateMaintenanceStatusDto dto);

    //History
    Task<List<MaintenanceStatusHistory>> GetStatusHistoryAsync(
    int maintenanceRequestId);

    //Expenses
    Task<MaintenanceExpense?> AddExpenseAsync(
    int maintenanceRequestId,
    CreateMaintenanceExpenseDto dto);

    Task<List<MaintenanceExpense>> GetExpensesAsync(
    int maintenanceRequestId);

    

    }
}