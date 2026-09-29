using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.DTOs.Viewing;
using PropMate.Api.Enums;
using PropMate.Api.Models;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/viewing-bookings")]
[Authorize]
public class ViewingBookingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ViewingBookingsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/viewing-bookings/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var bookings = await _context.ViewingBookings
            .Where(b => b.UserId == userId.Value)
            .OrderByDescending(b => b.BookedAt)
            .Select(b => new ViewingBookingDto
            {
                Id = b.Id,
                Status = b.Status,
                BookedAt = b.BookedAt,
                ViewingSlotId = b.ViewingSlotId,

                StartTime = b.ViewingSlot!.StartTime,
                EndTime = b.ViewingSlot.EndTime,

                PropertyListingId =
                    b.ViewingSlot.PropertyListingId,

                PropertyTitle =
                    b.ViewingSlot.PropertyListing!.Title,

                PropertyCity =
                    b.ViewingSlot.PropertyListing.City ?? string.Empty,

                PrimaryImageUrl = _context.PropertyImages
                    .Where(i =>
                        i.PropertyListingId ==
                            b.ViewingSlot.PropertyListingId &&
                        i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(bookings);
    }

    // POST /api/viewing-bookings
    [HttpPost]
    public async Task<IActionResult> BookViewing(
        [FromBody] CreateViewingBookingRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var slot = await _context.ViewingSlots
            .Include(v => v.PropertyListing)
            .FirstOrDefaultAsync(
                v => v.Id == request.ViewingSlotId);

        if (slot == null)
        {
            return NotFound(new
            {
                message = "Viewing slot not found."
            });
        }

        if (slot.PropertyListing == null ||
            slot.PropertyListing.Status != ListingStatus.Published)
        {
            return NotFound(new
            {
                message = "Published property listing not found."
            });
        }

        if (!slot.IsAvailable)
        {
            return Conflict(new
            {
                message = "Viewing slot is not available."
            });
        }

        var activeBookingExists =
            await _context.ViewingBookings
                .AnyAsync(b =>
                    b.ViewingSlotId == request.ViewingSlotId &&
                    b.Status == BookingStatus.Booked);

        if (activeBookingExists)
        {
            return Conflict(new
            {
                message = "Viewing slot has already been booked."
            });
        }

        var booking = new ViewingBooking
        {
            ViewingSlotId = request.ViewingSlotId,
            UserId = userId.Value,
            BookedAt = DateTime.UtcNow,
            Status = BookingStatus.Booked
        };

        slot.IsAvailable = false;

        _context.ViewingBookings.Add(booking);
        await _context.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                booking.Id,
                booking.ViewingSlotId,
                booking.BookedAt,
                booking.Status
            });
    }

    // POST /api/viewing-bookings/{id}/cancel
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelViewing(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var booking = await _context.ViewingBookings
            .Include(b => b.ViewingSlot)
            .FirstOrDefaultAsync(b =>
                b.Id == id &&
                b.UserId == userId.Value);

        if (booking == null)
        {
            return NotFound(new
            {
                message = "Viewing booking not found."
            });
        }

        if (booking.Status != BookingStatus.Booked)
        {
            return Conflict(new
            {
                message = "Only active bookings can be cancelled."
            });
        }

        booking.Status = BookingStatus.Cancelled;

        if (booking.ViewingSlot != null)
        {
            booking.ViewingSlot.IsAvailable = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Viewing booking cancelled."
        });
    }

    private int? GetCurrentUserId()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }
}

public class CreateViewingBookingRequest
{
    public int ViewingSlotId { get; set; }
}