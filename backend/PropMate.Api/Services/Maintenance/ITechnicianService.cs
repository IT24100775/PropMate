using PropMate.Api.DTOs.Maintenance;
using PropMate.Api.Models.Maintenance;

namespace PropMate.Api.Services.Maintenance
{
    public interface ITechnicianService
    {
        // Create
        Task<Technician> CreateAsync(CreateTechnicianDto dto);

        // Get all with search, filter, sort and pagination
        Task<(List<Technician> Items, int TotalCount)>
            GetAllAsync(TechnicianQueryDto query);

        // Get by ID
        Task<Technician?> GetByIdAsync(int id);

        // Update
        Task<Technician?> UpdateAsync(
            int id,
            UpdateTechnicianDto dto);

        // Delete
        Task<bool> DeleteAsync(int id);

        // Get available technicians
        Task<List<Technician>> GetAvailableAsync();
    }
}