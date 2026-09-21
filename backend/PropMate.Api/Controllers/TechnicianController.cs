using Microsoft.AspNetCore.Mvc;
using PropMate.Api.DTOs.Maintenance;
using PropMate.Api.Services.Maintenance;

namespace PropMate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechnicianController : ControllerBase
    {
        private readonly ITechnicianService _technicianService;

        public TechnicianController(
            ITechnicianService technicianService)
        {
            _technicianService = technicianService;
        }

        // POST: api/Technician
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateTechnicianDto dto)
        {
            var technician =
                await _technicianService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = technician.Id },
                technician
            );
        }

        // GET: api/Technician
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] TechnicianQueryDto query)
        {
            var result =
                await _technicianService.GetAllAsync(query);

            return Ok(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                page = query.Page < 1 ? 1 : query.Page,
                pageSize = query.PageSize < 1 ? 10 : query.PageSize
            });
        }

        // GET: api/Technician/available
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var technicians =
                await _technicianService.GetAvailableAsync();

            return Ok(technicians);
        }

        // GET: api/Technician/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var technician =
                await _technicianService.GetByIdAsync(id);

            if (technician == null)
            {
                return NotFound(new
                {
                    message = "Technician not found."
                });
            }

            return Ok(technician);
        }

        // PUT: api/Technician/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateTechnicianDto dto)
        {
            var technician =
                await _technicianService.UpdateAsync(id, dto);

            if (technician == null)
            {
                return NotFound(new
                {
                    message = "Technician not found."
                });
            }

            return Ok(technician);
        }

        // DELETE: api/Technician/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted =
                await _technicianService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Technician not found."
                });
            }

            return NoContent();
        }
    }
}