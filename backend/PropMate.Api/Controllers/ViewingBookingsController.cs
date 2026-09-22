using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.Models;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/viewing-bookings")]
public class ViewingBookingsController : ControllerBase
{
    private readonly PropMateDbContext _context;

    public ViewingBookingsController(PropMateDbContext context)
    {
        _context = context;
    }

    // POST /api/viewing-bookings
    [HttpPost]
    public async Task<IActionResult> BookViewing([FromBody] CreateViewingBookingRequest request)
    {
        // 1. Check if viewing slot exists
        var slot = await _context.ViewingSlots.FindAsync(request.ViewingSlotId);
        if (slot == null)
        {
            return NotFound(new { message = "Viewing slot not found." });
        }

        // 2. Check if the slot has already been booked
        var alreadyBooked = await _context.ViewingBookings
            .AnyAsync(b => b.ViewingSlotId == request.ViewingSlotId);

        if (alreadyBooked)
        {
            return Conflict(new { message = "Viewing slot has already been booked." });
        }

        // 3. Check if the slot is currently available
        if (!slot.IsAvailable)
        {
            return Conflict(new { message = "Viewing slot is not available." });
        }

        // 4. Create ViewingBooking and mark slot as unavailable
        var booking = new ViewingBooking
        {
            ViewingSlotId = request.ViewingSlotId,
            UserId = request.UserId,
            BookedAt = DateTime.UtcNow
        };

        slot.IsAvailable = false;

        _context.ViewingBookings.Add(booking);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, booking);
    }

    // DELETE /api/viewing-bookings/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelViewing(int id)
    {
        // 1. Find the booking by its ID
        var booking = await _context.ViewingBookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound(new { message = "Viewing booking not found." });
        }

        // 2. Find the related ViewingSlot
        var slot = await _context.ViewingSlots.FindAsync(booking.ViewingSlotId);
        if (slot != null)
        {
            // 3. Make the viewing slot available again
            slot.IsAvailable = true;
        }

        // 4. Delete the booking
        _context.ViewingBookings.Remove(booking);

        // 5. Save the changes asynchronously
        await _context.SaveChangesAsync();

        // 6. Return 204 No Content
        return NoContent();
    }
}

public class CreateViewingBookingRequest
{
    public int ViewingSlotId { get; set; }
    public int UserId { get; set; }
}
