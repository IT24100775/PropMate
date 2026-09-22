using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.Models;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavouritesController : ControllerBase
{
    private readonly PropMateDbContext _context;

    public FavouritesController(PropMateDbContext context)
    {
        _context = context;
    }

    // POST /api/favourites
    [HttpPost]
    public async Task<IActionResult> AddFavourite([FromBody] AddFavouriteRequest request)
    {
        // Check if property exists
        var propertyExists = await _context.Properties.AnyAsync(p => p.Id == request.PropertyId);
        if (!propertyExists)
        {
            return NotFound(new { message = "Property not found." });
        }

        // Check if the user already favourited this property
        var existingFavourite = await _context.Favourites
            .FirstOrDefaultAsync(f => f.PropertyId == request.PropertyId && f.UserId == request.UserId);

        if (existingFavourite != null)
        {
            return Conflict(new { message = "Property is already favourited by this user." });
        }

        var favourite = new Favourite
        {
            PropertyId = request.PropertyId,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Favourites.Add(favourite);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(AddFavourite), new { id = favourite.Id }, favourite);
    }

    // DELETE /api/favourites/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFavourite(int id)
    {
        var favourite = await _context.Favourites.FindAsync(id);
        if (favourite == null)
        {
            return NotFound(new { message = "Favourite not found." });
        }

        _context.Favourites.Remove(favourite);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class AddFavouriteRequest
{
    public int PropertyId { get; set; }
    public int UserId { get; set; }
}
