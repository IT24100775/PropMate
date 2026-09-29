using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.Enums;
using PropMate.Api.Models;

namespace PropMate.Api.Controllers;

[ApiController]
public class ViewingSlotsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ViewingSlotsController(AppDbContext context)
    {
        _context = context;
    }

    // Supports both the existing API route and the route used by Flutter.
    //
    // GET /api/properties/{propertyId}/viewing-slots
    // GET /api/viewing-slots/property/{propertyId}/available
    [HttpGet("api/properties/{propertyId}/viewing-slots")]
    [HttpGet("api/viewing-slots/property/{propertyId}/available")]
    public async Task<IActionResult> GetViewingSlotsForProperty(int propertyId)
    {
        var propertyExists = await _context.PropertyListings
            .AnyAsync(p =>
                p.Id == propertyId &&
                p.Status == ListingStatus.Published);

        if (!propertyExists)
        {
            return NotFound(new
            {
                message = "Published property listing not found."
            });
        }

        var slots = await _context.ViewingSlots
            .Where(v =>
                v.PropertyListingId == propertyId &&
                v.IsAvailable)
            .Select(v => new
            {
                v.Id,
                PropertyId = v.PropertyListingId,
                v.StartTime,
                v.EndTime,
                v.IsAvailable
            })
            .ToListAsync();

        return Ok(slots);
    }

    // POST /api/viewing-slots
    [HttpPost("api/viewing-slots")]
    public async Task<IActionResult> CreateViewingSlot(
        [FromBody] CreateViewingSlotRequest request)
    {
        var propertyExists = await _context.PropertyListings
            .AnyAsync(p => p.Id == request.PropertyId);

        if (!propertyExists)
        {
            return NotFound(new
            {
                message = "Property listing not found."
            });
        }

        var startTimeUtc = request.StartTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc)
            : request.StartTime.ToUniversalTime();

        var endTimeUtc = request.EndTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc)
            : request.EndTime.ToUniversalTime();

        if (endTimeUtc <= startTimeUtc)
        {
            return BadRequest(new
            {
                message = "End time must be after start time."
            });
        }

        var slot = new ViewingSlot
        {
            PropertyListingId = request.PropertyId,
            StartTime = startTimeUtc,
            EndTime = endTimeUtc,
            IsAvailable = true
        };

        _context.ViewingSlots.Add(slot);
        await _context.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                slot.Id,
                PropertyId = slot.PropertyListingId,
                slot.StartTime,
                slot.EndTime,
                slot.IsAvailable
            });
    }

    // PUT /api/viewing-slots/{id}
    [HttpPut("api/viewing-slots/{id}")]
    public async Task<IActionResult> UpdateViewingSlot(
        int id,
        [FromBody] CreateViewingSlotRequest request)
    {
        var slot = await _context.ViewingSlots.FindAsync(id);

        if (slot == null)
        {
            return NotFound(new
            {
                message = "Viewing slot not found."
            });
        }

        var propertyExists = await _context.PropertyListings
            .AnyAsync(p => p.Id == request.PropertyId);

        if (!propertyExists)
        {
            return NotFound(new
            {
                message = "Property listing not found."
            });
        }

        var startTimeUtc = request.StartTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc)
            : request.StartTime.ToUniversalTime();

        var endTimeUtc = request.EndTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc)
            : request.EndTime.ToUniversalTime();

        if (endTimeUtc <= startTimeUtc)
        {
            return BadRequest(new
            {
                message = "End time must be after start time."
            });
        }

        slot.PropertyListingId = request.PropertyId;
        slot.StartTime = startTimeUtc;
        slot.EndTime = endTimeUtc;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            slot.Id,
            PropertyId = slot.PropertyListingId,
            slot.StartTime,
            slot.EndTime,
            slot.IsAvailable
        });
    }

    // DELETE /api/viewing-slots/{id}
    [HttpDelete("api/viewing-slots/{id}")]
    public async Task<IActionResult> DeleteViewingSlot(int id)
    {
        var slot = await _context.ViewingSlots.FindAsync(id);

        if (slot == null)
        {
            return NotFound(new
            {
                message = "Viewing slot not found."
            });
        }

        _context.ViewingSlots.Remove(slot);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateViewingSlotRequest
{
    public int PropertyId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
}