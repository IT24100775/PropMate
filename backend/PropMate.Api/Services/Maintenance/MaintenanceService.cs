using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.DTOs.Maintenance;
using PropMate.Api.Models.Maintenance;

namespace PropMate.Api.Services.Maintenance
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE
        public async Task<MaintenanceRequest> CreateAsync(
            CreateMaintenanceRequestDto dto)
        {
            var request = new MaintenanceRequest
            {
                PropertyId = dto.PropertyId,
                TenantId = dto.TenantId,
                Description = dto.Description,
                Category = dto.Category,
                Priority = dto.Priority,
                ImageUrl = dto.ImageUrl,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MaintenanceRequests.Add(request);

            await _context.SaveChangesAsync();

            return request;
        }

        // GET ALL WITH SEARCH, FILTER, SORT AND PAGINATION
        public async Task<(List<MaintenanceRequest> Items, int TotalCount)>
            GetAllAsync(MaintenanceQueryDto query)
        {
            var requests = _context.MaintenanceRequests
                .AsNoTracking()
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();

                requests = requests.Where(x =>
                    x.Description.ToLower().Contains(search) ||
                    x.Category.ToLower().Contains(search));
            }

            // FILTER BY STATUS
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                requests = requests.Where(x =>
                    x.Status == query.Status);
            }

            // FILTER BY PRIORITY
            if (!string.IsNullOrWhiteSpace(query.Priority))
            {
                requests = requests.Where(x =>
                    x.Priority == query.Priority);
            }

            // FILTER BY CATEGORY
            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                requests = requests.Where(x =>
                    x.Category == query.Category);
            }

            // FILTER BY PROPERTY
            if (query.PropertyId.HasValue)
            {
                requests = requests.Where(x =>
                    x.PropertyId == query.PropertyId.Value);
            }

            // TOTAL COUNT BEFORE PAGINATION
            var totalCount = await requests.CountAsync();

            // SORT
            if (query.SortBy.ToLower() == "priority")
            {
                requests = query.SortOrder.ToLower() == "asc"
                    ? requests.OrderBy(x => x.Priority)
                    : requests.OrderByDescending(x => x.Priority);
            }
            else if (query.SortBy.ToLower() == "status")
            {
                requests = query.SortOrder.ToLower() == "asc"
                    ? requests.OrderBy(x => x.Status)
                    : requests.OrderByDescending(x => x.Status);
            }
            else
            {
                requests = query.SortOrder.ToLower() == "asc"
                    ? requests.OrderBy(x => x.CreatedAt)
                    : requests.OrderByDescending(x => x.CreatedAt);
            }

            // VALIDATE PAGE VALUES
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

            // PAGINATION
            var items = await requests
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // GET BY ID
        public async Task<MaintenanceRequest?> GetByIdAsync(int id)
        {
            return await _context.MaintenanceRequests
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // UPDATE
        public async Task<MaintenanceRequest?> UpdateAsync(
            int id,
            UpdateMaintenanceRequestDto dto)
        {
            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                return null;
            }

            request.Description = dto.Description;
            request.Category = dto.Category;
            request.Priority = dto.Priority;
            request.ImageUrl = dto.ImageUrl;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return request;
        }


        public async Task<MaintenanceAssignment?> AssignTechnicianAsync(
    int maintenanceRequestId,
    AssignTechnicianDto dto)
{
    var request = await _context.MaintenanceRequests
        .FirstOrDefaultAsync(x => x.Id == maintenanceRequestId);

    if (request == null)
    {
        return null;
    }

    var technician = await _context.Technicians
        .FirstOrDefaultAsync(x => x.Id == dto.TechnicianId);

    if (technician == null)
    {
        throw new Exception("Technician not found.");
    }

    if (technician.AvailabilityStatus != "AVAILABLE")
    {
        throw new Exception("Technician is not available.");
    }

    var assignment = new MaintenanceAssignment
    {
        MaintenanceRequestId = maintenanceRequestId,
        TechnicianId = dto.TechnicianId,
        AssignedBy = dto.AssignedBy,
        AssignedAt = DateTime.UtcNow,
        Notes = dto.Notes
    };

    _context.MaintenanceAssignments.Add(assignment);

    request.Status = "ASSIGNED";
    request.UpdatedAt = DateTime.UtcNow;

    technician.AvailabilityStatus = "BUSY";
    technician.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return assignment;
}

        // DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                return false;
            }

            _context.MaintenanceRequests.Remove(request);

            await _context.SaveChangesAsync();

            return true;
        }

        //Sceduling
        public async Task<RepairSchedule?> ScheduleRepairAsync(
    int maintenanceRequestId,
    ScheduleRepairDto dto)
{
    var request = await _context.MaintenanceRequests
        .FirstOrDefaultAsync(x => x.Id == maintenanceRequestId);

    if (request == null)
    {
        return null;
    }

    if (request.Status != "ASSIGNED")
    {
        throw new Exception(
            "Maintenance request must be ASSIGNED before scheduling.");
    }

    var technician = await _context.Technicians
        .FirstOrDefaultAsync(x => x.Id == dto.TechnicianId);

    if (technician == null)
    {
        throw new Exception("Technician not found.");
    }

    if (dto.StartTime >= dto.EndTime)
    {
        throw new Exception(
            "Start time must be earlier than end time.");
    }

    var assignment = await _context.MaintenanceAssignments
        .FirstOrDefaultAsync(x =>
            x.MaintenanceRequestId == maintenanceRequestId &&
            x.TechnicianId == dto.TechnicianId);

    if (assignment == null)
    {
        throw new Exception(
            "This technician is not assigned to this maintenance request.");
    }

   var schedule = new RepairSchedule
{
    MaintenanceRequestId = maintenanceRequestId,
    TechnicianId = dto.TechnicianId,
    ScheduledDate = DateTime.SpecifyKind(
        dto.ScheduledDate,
        DateTimeKind.Utc
    ),
    StartTime = dto.StartTime,
    EndTime = dto.EndTime,
    Notes = dto.Notes,
    CreatedAt = DateTime.UtcNow
};

    _context.RepairSchedules.Add(schedule);

    request.Status = "SCHEDULED";
    request.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return schedule;
}

        //Status workflow
        public async Task<MaintenanceRequest?> UpdateStatusAsync(
    int maintenanceRequestId,
    UpdateMaintenanceStatusDto dto)
{
    var request = await _context.MaintenanceRequests
        .FirstOrDefaultAsync(x => x.Id == maintenanceRequestId);

    if (request == null)
    {
        return null;
    }

    var allowedTransitions = new Dictionary<string, string[]>
    {
        { "PENDING", new[] { "ASSIGNED", "REJECTED" } },
        { "ASSIGNED", new[] { "SCHEDULED", "CANCELLED" } },
        { "SCHEDULED", new[] { "IN_PROGRESS", "CANCELLED" } },
        { "IN_PROGRESS", new[] { "RESOLVED" } },
        { "RESOLVED", new[] { "REOPENED" } },
        { "REOPENED", new[] { "ASSIGNED" } }
    };

    if (!allowedTransitions.ContainsKey(request.Status))
    {
        throw new Exception(
            $"Status '{request.Status}' cannot be changed.");
    }

    if (!allowedTransitions[request.Status]
        .Contains(dto.Status))
    {
        throw new Exception(
            $"Cannot change status from '{request.Status}' to '{dto.Status}'.");
    }

    var oldStatus = request.Status;

request.Status = dto.Status;
request.UpdatedAt = DateTime.UtcNow;

var history = new MaintenanceStatusHistory
{
    MaintenanceRequestId = maintenanceRequestId,
    OldStatus = oldStatus,
    NewStatus = dto.Status,
    ChangedBy = dto.ChangedBy,
    Comment = dto.Comment,
    ChangedAt = DateTime.UtcNow
};

_context.MaintenanceStatusHistories.Add(history);

    if (dto.Status == "RESOLVED")
    {
        request.ResolvedAt = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();

    return request;
}

//History
public async Task<List<MaintenanceStatusHistory>> GetStatusHistoryAsync(
    int maintenanceRequestId)
{
    return await _context.MaintenanceStatusHistories
        .AsNoTracking()
        .Where(x => x.MaintenanceRequestId == maintenanceRequestId)
        .OrderBy(x => x.ChangedAt)
        .ToListAsync();
}

//Expenses
public async Task<MaintenanceExpense?> AddExpenseAsync(
    int maintenanceRequestId,
    CreateMaintenanceExpenseDto dto)
{
    var request = await _context.MaintenanceRequests
        .FirstOrDefaultAsync(x => x.Id == maintenanceRequestId);

    if (request == null)
    {
        return null;
    }

    if (dto.Amount <= 0)
    {
        throw new Exception("Expense amount must be greater than zero.");
    }

    var expense = new MaintenanceExpense
    {
        MaintenanceRequestId = maintenanceRequestId,
        Amount = dto.Amount,
        Description = dto.Description,
        RecordedBy = dto.RecordedBy,
        CreatedAt = DateTime.UtcNow
    };

    _context.MaintenanceExpenses.Add(expense);

    await _context.SaveChangesAsync();

    return expense;
}
    }
}