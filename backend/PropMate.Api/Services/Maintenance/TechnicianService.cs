using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.DTOs.Maintenance;
using PropMate.Api.Models.Maintenance;

namespace PropMate.Api.Services.Maintenance
{
    public class TechnicianService : ITechnicianService
    {
        private readonly ApplicationDbContext _context;

        public TechnicianService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE
        public async Task<Technician> CreateAsync(
            CreateTechnicianDto dto)
        {
            var technician = new Technician
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                Specialization = dto.Specialization,
                AvailabilityStatus = dto.AvailabilityStatus,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Technicians.Add(technician);

            await _context.SaveChangesAsync();

            return technician;
        }

        // GET ALL WITH SEARCH, FILTER, SORT AND PAGINATION
        public async Task<(List<Technician> Items, int TotalCount)>
            GetAllAsync(TechnicianQueryDto query)
        {
            var technicians = _context.Technicians
                .AsNoTracking()
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();

                technicians = technicians.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    x.Email.ToLower().Contains(search) ||
                    x.Specialization.ToLower().Contains(search));
            }

            // FILTER BY SPECIALIZATION
            if (!string.IsNullOrWhiteSpace(query.Specialization))
            {
                technicians = technicians.Where(x =>
                    x.Specialization == query.Specialization);
            }

            // FILTER BY AVAILABILITY
            if (!string.IsNullOrWhiteSpace(query.AvailabilityStatus))
            {
                technicians = technicians.Where(x =>
                    x.AvailabilityStatus == query.AvailabilityStatus);
            }

            // TOTAL COUNT BEFORE PAGINATION
            var totalCount = await technicians.CountAsync();

            // SORT
            if (query.SortBy.ToLower() == "specialization")
            {
                technicians = query.SortOrder.ToLower() == "desc"
                    ? technicians.OrderByDescending(x => x.Specialization)
                    : technicians.OrderBy(x => x.Specialization);
            }
            else if (query.SortBy.ToLower() == "availabilitystatus")
            {
                technicians = query.SortOrder.ToLower() == "desc"
                    ? technicians.OrderByDescending(x => x.AvailabilityStatus)
                    : technicians.OrderBy(x => x.AvailabilityStatus);
            }
            else
            {
                technicians = query.SortOrder.ToLower() == "desc"
                    ? technicians.OrderByDescending(x => x.Name)
                    : technicians.OrderBy(x => x.Name);
            }

            // VALIDATE PAGE VALUES
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

            // PAGINATION
            var items = await technicians
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // GET BY ID
        public async Task<Technician?> GetByIdAsync(int id)
        {
            return await _context.Technicians
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // UPDATE
        public async Task<Technician?> UpdateAsync(
            int id,
            UpdateTechnicianDto dto)
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(x => x.Id == id);

            if (technician == null)
            {
                return null;
            }

            technician.Name = dto.Name;
            technician.Phone = dto.Phone;
            technician.Email = dto.Email;
            technician.Specialization = dto.Specialization;
            technician.AvailabilityStatus = dto.AvailabilityStatus;
            technician.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return technician;
        }

        // DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(x => x.Id == id);

            if (technician == null)
            {
                return false;
            }

            _context.Technicians.Remove(technician);

            await _context.SaveChangesAsync();

            return true;
        }

        // GET AVAILABLE TECHNICIANS
        public async Task<List<Technician>> GetAvailableAsync()
        {
            return await _context.Technicians
                .AsNoTracking()
                .Where(x => x.AvailabilityStatus == "AVAILABLE")
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}