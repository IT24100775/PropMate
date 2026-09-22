using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.Models;

namespace PropMate.Api.Controllers;

[ApiController]
public class ViewingSlotsController : ControllerBase
{
    private readonly PropMateDbContext _context;

    public ViewingSlotsController(PropMateDbContext context)
    {
        _context = context;
    }

    // GET /api/properties/{propertyId}/viewing-slots
    [HttpGet("api/properties/{propertyId}/viewing-slots")]
    public async Task<IActionResult> GetViewingSlotsForProperty(int propertyId)
    {
        var propertyExists = await _context.Properties.AnyAsync(p => p.Id == propertyId);
        if (!propertyExists)
        {
            return NotFound(new { message = "Property not found." });
        }

        var slots = await _context.ViewingSlots
            .Where(v => v.PropertyId == propertyId && v.IsAvailable)
            .Select(v => new
            {
                v.Id,
                v.PropertyId,
                v.StartTime,
                v.EndTime,
                v.IsAvailable
            })
            .ToListAsync();

        return Ok(slots);
    }

    // POST /api/viewing-slots
    [HttpPost("api/viewing-slots")]
    public async Task<IActionResult> CreateViewingSlot([FromBody] CreateViewingSlotRequest request)
    {
        var propertyExists = await _context.Properties.AnyAsync(p => p.Id == request.PropertyId);
        if (!propertyExists)
        {
            return NotFound(new { message = "Property not found." });
        }

        var startTimeUtc = request.StartTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc)
            : request.StartTime.ToUniversalTime();

        var endTimeUtc = request.EndTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc)
            : request.EndTime.ToUniversalTime();

        var slot = new ViewingSlot
        {
            PropertyId = request.PropertyId,
            StartTime = startTimeUtc,
            EndTime = endTimeUtc,
            IsAvailable = true
        };

        _context.ViewingSlots.Add(slot);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, slot);
    }
}

public class CreateViewingSlotRequest
{
    public int PropertyId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
