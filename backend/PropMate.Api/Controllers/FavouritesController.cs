using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.Enums;
using PropMate.Api.Models;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavouritesController : ControllerBase
{
    private readonly AppDbContext _context;

    public FavouritesController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/favourites?page=1
    [HttpGet]
    public async Task<IActionResult> GetFavourites(
        [FromQuery] int page = 1)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        if (page < 1)
        {
            page = 1;
        }

        const int pageSize = 10;

        var query = _context.Favourites
            .Where(f => f.UserId == userId.Value)
            .Join(
                _context.PropertyListings,
                favourite => favourite.PropertyListingId,
                property => property.Id,
                (favourite, property) => property)
            .Where(p => p.Status == ListingStatus.Published)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                id = p.Id,
                title = p.Title,
                description = p.Description,
                price = p.Price,
                purpose = p.Purpose.ToString(),
                bedrooms = p.Bedrooms,
                bathrooms = p.Bathrooms,
                address = p.Address,
                city = p.City,
                latitude = p.Latitude,
                longitude = p.Longitude,
                imageUrls = p.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .ToList(),
                isAvailable = true
            })
            .ToListAsync();

        return Ok(new
        {
            items,
            page,
            totalCount,
            totalPages
        });
    }

    // POST /api/favourites/{propertyListingId}
    [HttpPost("{propertyListingId:int}")]
    public async Task<IActionResult> AddFavourite(
        int propertyListingId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var propertyExists = await _context.PropertyListings
            .AnyAsync(p =>
                p.Id == propertyListingId &&
                p.Status == ListingStatus.Published);

        if (!propertyExists)
        {
            return NotFound(new
            {
                message = "Published property listing not found."
            });
        }

        var existingFavourite = await _context.Favourites
            .AnyAsync(f =>
                f.PropertyListingId == propertyListingId &&
                f.UserId == userId.Value);

        if (existingFavourite)
        {
            return Conflict(new
            {
                message = "Property is already favourited."
            });
        }

        var favourite = new Favourite
        {
            PropertyListingId = propertyListingId,
            UserId = userId.Value,
            CreatedAt = DateTime.UtcNow
        };

        _context.Favourites.Add(favourite);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created);
    }

    // DELETE /api/favourites/{propertyListingId}
    [HttpDelete("{propertyListingId:int}")]
    public async Task<IActionResult> DeleteFavourite(
        int propertyListingId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var favourite = await _context.Favourites
            .FirstOrDefaultAsync(f =>
                f.PropertyListingId == propertyListingId &&
                f.UserId == userId.Value);

        if (favourite == null)
        {
            return NotFound(new
            {
                message = "Favourite not found."
            });
        }

        _context.Favourites.Remove(favourite);
        await _context.SaveChangesAsync();

        return NoContent();
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