using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.Models;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly PropMateDbContext _context;

    public PropertiesController(PropMateDbContext context)
    {
        _context = context;
    }

    // GET /api/properties
    // Supported query parameters: search, minPrice, maxPrice, bedrooms, bathrooms, location, isAvailable
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Property>>> GetProperties(
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? bedrooms,
        [FromQuery] int? bathrooms,
        [FromQuery] string? location,
        [FromQuery] bool? isAvailable)
    {
        var query = _context.Properties.AsQueryable();

        // Keyword search in Title or Location (case-insensitive)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(searchLower) ||
                                     p.Location.ToLower().Contains(searchLower));
        }

        // Filter by minimum price
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        // Filter by maximum price
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Filter by minimum bedrooms
        if (bedrooms.HasValue)
        {
            query = query.Where(p => p.Bedrooms >= bedrooms.Value);
        }

        // Filter by minimum bathrooms
        if (bathrooms.HasValue)
        {
            query = query.Where(p => p.Bathrooms >= bathrooms.Value);
        }

        // Filter by location (case-insensitive)
        if (!string.IsNullOrWhiteSpace(location))
        {
            var locationLower = location.Trim().ToLower();
            query = query.Where(p => p.Location.ToLower().Contains(locationLower));
        }

        // Filter by availability
        if (isAvailable.HasValue)
        {
            query = query.Where(p => p.IsAvailable == isAvailable.Value);
        }

        var properties = await query.ToListAsync();
        return Ok(properties);
    }

    // GET /api/properties/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Property>> GetProperty(int id)
    {
        var property = await _context.Properties.FindAsync(id);

        if (property == null)
        {
            return NotFound();
        }

        return Ok(property);
    }

    // GET /api/properties/{id}/location
    [HttpGet("{id}/location")]
    public async Task<IActionResult> GetPropertyLocation(int id)
    {
        var locationData = await _context.Properties
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Location,
                p.Latitude,
                p.Longitude
            })
            .FirstOrDefaultAsync();

        if (locationData == null)
        {
            return NotFound();
        }

        return Ok(locationData);
    }
}
