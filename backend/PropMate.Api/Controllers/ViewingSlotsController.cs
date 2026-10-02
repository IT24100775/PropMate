using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

    // GET /api/viewing-slots/property/{propertyId}/manage
// Owner/Agent: get all viewing slots for one of their properties,
// including booked/unavailable slots.
[Authorize(Roles = "OwnerAgent")]
[HttpGet("api/viewing-slots/property/{propertyId}/manage")]
public async Task<IActionResult> GetViewingSlotsForManagement(int propertyId)
{
    var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!int.TryParse(userIdValue, out var ownerId))
    {
        return Unauthorized();
    }

    var property = await _context.PropertyListings
        .FirstOrDefaultAsync(p =>
            p.Id == propertyId &&
            p.OwnerId == ownerId);

    if (property == null)
    {
        return NotFound(new
        {
            message = "Property listing not found or you do not own this property."
        });
    }

    var slots = await _context.ViewingSlots
        .Where(v => v.PropertyListingId == propertyId)
        .OrderBy(v => v.StartTime)
        .Select(v => new
        {
            v.Id,
            PropertyId = v.PropertyListingId,
            v.StartTime,
            v.EndTime,
            v.IsAvailable,

            Booking = _context.ViewingBookings
                .Where(b =>
                    b.ViewingSlotId == v.Id &&
                    b.Status == BookingStatus.Booked)
                .Select(b => new
                {
                    b.Id,
                    b.UserId,
                    b.BookedAt,
                    b.Status,
                    UserEmail = b.User != null
                        ? b.User.Email
                        : null
                })
                .FirstOrDefault()
        })
        .ToListAsync();

    return Ok(new
    {
        propertyId = property.Id,
        propertyTitle = property.Title,
        slots
    });
}

    // POST /api/viewing-slots
    [Authorize(Roles = "OwnerAgent")]
    [HttpPost("api/viewing-slots")]
    public async Task<IActionResult> CreateViewingSlot(
        [FromBody] CreateViewingSlotRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

if (!int.TryParse(userIdValue, out var ownerId))
{
    return Unauthorized();
}

var propertyExists = await _context.PropertyListings
    .AnyAsync(p =>
        p.Id == request.PropertyId &&
        p.OwnerId == ownerId);

if (!propertyExists)
{
    return NotFound(new
    {
        message = "Property listing not found or you do not own this property."
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
    [Authorize(Roles = "OwnerAgent")]
    [HttpPut("api/viewing-slots/{id}")]
    public async Task<IActionResult> UpdateViewingSlot(
        int id,
        [FromBody] CreateViewingSlotRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

if (!int.TryParse(userIdValue, out var ownerId))
{
    return Unauthorized();
}

var slot = await _context.ViewingSlots
    .Include(v => v.PropertyListing)
    .FirstOrDefaultAsync(v => v.Id == id);

if (slot == null ||
    slot.PropertyListing == null ||
    slot.PropertyListing.OwnerId != ownerId)
{
    return NotFound(new
    {
        message = "Viewing slot not found or you do not own this property."
    });
}

var propertyExists = await _context.PropertyListings
    .AnyAsync(p =>
        p.Id == request.PropertyId &&
        p.OwnerId == ownerId);

if (!propertyExists)
{
    return NotFound(new
    {
        message = "Property listing not found or you do not own this property."
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
    [Authorize(Roles = "OwnerAgent")]
    [HttpDelete("api/viewing-slots/{id}")]
    public async Task<IActionResult> DeleteViewingSlot(int id)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

if (!int.TryParse(userIdValue, out var ownerId))
{
    return Unauthorized();
}

var slot = await _context.ViewingSlots
    .Include(v => v.PropertyListing)
    .FirstOrDefaultAsync(v => v.Id == id);

if (slot == null ||
    slot.PropertyListing == null ||
    slot.PropertyListing.OwnerId != ownerId)
{
    return NotFound(new
    {
        message = "Viewing slot not found or you do not own this property."
    });
}

var hasActiveBooking = await _context.ViewingBookings
    .AnyAsync(b =>
        b.ViewingSlotId == id &&
        b.Status == BookingStatus.Booked);

if (hasActiveBooking)
{
    return Conflict(new
    {
        message = "A booked viewing slot cannot be deleted."
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