using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropMate.Api.DTOs.Maintenance;
using PropMate.Api.Services.Maintenance;

namespace PropMate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        // POST: api/Maintenance
        [Authorize(Roles = "PropertyManager,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateMaintenanceRequestDto dto)
        {
            var request = await _maintenanceService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = request.Id },
                request
            );
        }

        [Authorize(Roles = "BuyerRenter")]
        [HttpPost("tenant")]
        public async Task<IActionResult> CreateForTenant(
            CreateTenantMaintenanceRequestDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var tenantId))
            {
                return Unauthorized();
            }

            try
            {
                var request = await _maintenanceService.CreateForTenantAsync(tenantId, dto);
                return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Maintenance
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] MaintenanceQueryDto query)
        {
            var result = await _maintenanceService.GetAllAsync(query);

            return Ok(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                page = query.Page < 1 ? 1 : query.Page,
                pageSize = query.PageSize < 1 ? 10 : query.PageSize
            });
        }

        [Authorize(Roles = "BuyerRenter")]
        [HttpGet("tenant/{tenantId}")]
        public async Task<IActionResult> GetForTenant(int tenantId)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentTenantId) ||
                currentTenantId != tenantId)
            {
                return Forbid();
            }

            var result = await _maintenanceService.GetAllAsync(new MaintenanceQueryDto
            {
                TenantId = tenantId,
                Page = 1,
                PageSize = 100
            });

            return Ok(result.Items);
        }

        [Authorize(Roles = "BuyerRenter")]
        [HttpGet("tenant/{tenantId}/notifications")]
        public async Task<IActionResult> GetNotificationsForTenant(int tenantId)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentTenantId) ||
                currentTenantId != tenantId)
            {
                return Forbid();
            }

            var notifications = await _maintenanceService.GetNotificationsForTenantAsync(tenantId);
            return Ok(notifications);
        }

        // GET: api/Maintenance/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _maintenanceService.GetByIdAsync(id);

            if (request == null)
            {
                return NotFound(new
                {
                    message = "Maintenance request not found."
                });
            }

            return Ok(request);
        }

        // PUT: api/Maintenance/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateMaintenanceRequestDto dto)
        {
            var request = await _maintenanceService.UpdateAsync(id, dto);

            if (request == null)
            {
                return NotFound(new
                {
                    message = "Maintenance request not found."
                });
            }

            return Ok(request);
        }

        // DELETE: api/Maintenance/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _maintenanceService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Maintenance request not found."
                });
            }

            return NoContent();
        }

        //Scheduling 
        [HttpPost("{id}/schedule")]
public async Task<IActionResult> ScheduleRepair(
    int id,
    ScheduleRepairDto dto)
{
    try
    {
        var schedule =
            await _maintenanceService.ScheduleRepairAsync(id, dto);

        if (schedule == null)
        {
            return NotFound(new
            {
                message = "Maintenance request not found."
            });
        }

        return Ok(new
        {
            message = "Repair scheduled successfully.",
            schedule
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new
        {
            message = ex.Message
        });
    }
}

        [HttpPost("{id}/assign")]
public async Task<IActionResult> AssignTechnician(
    int id,
    AssignTechnicianDto dto)
{
    try
    {
        var assignment =
            await _maintenanceService.AssignTechnicianAsync(id, dto);

        if (assignment == null)
        {
            return NotFound(new
            {
                message = "Maintenance request not found."
            });
        }

        return Ok(new
        {
            message = "Technician assigned successfully.",
            assignment
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new
        {
            message = ex.Message
        });
    }
}

[HttpPost("{id}/ai/approve")]
public async Task<IActionResult> ApproveAiRecommendation(
    int id,
    ApproveMaintenanceAiDto dto)
{
    try
    {
        var result = await _maintenanceService.ApproveAiRecommendationAsync(id, dto);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Maintenance request not found."
            });
        }

        return Ok(new
        {
            message = "AI recommendation approved and executed by backend.",
            result
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new
        {
            message = ex.Message
        });
    }
}

//Status workflow
[HttpPatch("{id}/status")]
public async Task<IActionResult> UpdateStatus(
    int id,
    UpdateMaintenanceStatusDto dto)
{
    try
    {
        var request =
            await _maintenanceService.UpdateStatusAsync(id, dto);

        if (request == null)
        {
            return NotFound(new
            {
                message = "Maintenance request not found."
            });
        }

        return Ok(new
        {
            message = "Maintenance status updated successfully.",
            request
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new
        {
            message = ex.Message
        });
    }
}

//History
[HttpGet("{id}/history")]
public async Task<IActionResult> GetStatusHistory(int id)
{
    var request = await _maintenanceService.GetByIdAsync(id);

    if (request == null)
    {
        return NotFound(new
        {
            message = "Maintenance request not found."
        });
    }

    var history =
        await _maintenanceService.GetStatusHistoryAsync(id);

    return Ok(history);
}

//Expenses
[HttpPost("{id}/expenses")]
public async Task<IActionResult> AddExpense(
    int id,
    CreateMaintenanceExpenseDto dto)
{
    try
    {
        var expense =
            await _maintenanceService.AddExpenseAsync(id, dto);

        if (expense == null)
        {
            return NotFound(new
            {
                message = "Maintenance request not found."
            });
        }

        return Ok(new
        {
            message = "Maintenance expense added successfully.",
            expense
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new
        {
            message = ex.Message
        });
    }
}

[HttpGet("{id}/expenses")]
public async Task<IActionResult> GetExpenses(int id)
{
    var request = await _maintenanceService.GetByIdAsync(id);

    if (request == null)
    {
        return NotFound(new
        {
            message = "Maintenance request not found."
        });
    }

    return Ok(await _maintenanceService.GetExpensesAsync(id));
}

    }
}