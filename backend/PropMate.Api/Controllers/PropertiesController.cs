
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
    // Supported query parameters:
    // search, minPrice, maxPrice, bedrooms, bathrooms, location, isAvailable
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

        // Keyword search in Title or Location
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();

            query = query.Where(p =>
                p.Title.ToLower().Contains(searchLower) ||
                p.Location.ToLower().Contains(searchLower));
        }

        // Minimum price
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        // Maximum price
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Minimum bedrooms
        if (bedrooms.HasValue)
        {
            query = query.Where(p => p.Bedrooms >= bedrooms.Value);
        }

        // Minimum bathrooms
        if (bathrooms.HasValue)
        {
            query = query.Where(p => p.Bathrooms >= bathrooms.Value);
        }

        // Location
        if (!string.IsNullOrWhiteSpace(location))
        {
            var locationLower = location.Trim().ToLower();

            query = query.Where(p =>
                p.Location.ToLower().Contains(locationLower));
        }

        // Availability
        if (isAvailable.HasValue)
        {
            query = query.Where(p =>
                p.IsAvailable == isAvailable.Value);
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

    // POST /api/properties/natural-search
    [HttpPost("natural-search")]
    public async Task<ActionResult<IEnumerable<Property>>> NaturalLanguageSearch(
        [FromBody] NaturalLanguageSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest("Search query is required.");
        }

        var search = request.Query.Trim().ToLower();

        var query = _context.Properties.AsQueryable();

        // Simple natural-language keyword matching
        if (search.Contains("colombo"))
        {
            query = query.Where(p =>
                p.Location.ToLower().Contains("colombo"));
        }

        if (search.Contains("kandy"))
        {
            query = query.Where(p =>
                p.Location.ToLower().Contains("kandy"));
        }

        if (search.Contains("bedroom"))
        {
            var numbers = System.Text.RegularExpressions.Regex
                .Matches(search, @"\d+")
                .Select(m => int.Parse(m.Value))
                .ToList();

            if (numbers.Count > 0)
            {
                query = query.Where(p =>
                    p.Bedrooms >= numbers[0]);
            }
        }

        if (search.Contains("million"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                search,
                @"(\d+(?:\.\d+)?)\s*million");

            if (match.Success)
            {
                var millions = decimal.Parse(match.Groups[1].Value);
                var maxPrice = millions * 1_000_000;

                if (search.Contains("under") ||
                    search.Contains("below") ||
                    search.Contains("less"))
                {
                    query = query.Where(p =>
                        p.Price <= maxPrice);
                }
            }
        }

        var properties = await query.ToListAsync();

        return Ok(properties);
    }

    // POST /api/properties
    // Staff: Create a new property
    [HttpPost]
    public async Task<ActionResult<Property>> CreateProperty(
        [FromBody] Property property)
    {
        if (property == null)
        {
            return BadRequest("Property data is required.");
        }

        _context.Properties.Add(property);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProperty),
            new { id = property.Id },
            property);
    }

    // PUT /api/properties/{id}
    // Staff: Update an existing property
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProperty(
        int id,
        [FromBody] Property property)
    {
        if (id != property.Id)
        {
            return BadRequest("Property ID mismatch.");
        }

        var existingProperty =
            await _context.Properties.FindAsync(id);

        if (existingProperty == null)
        {
            return NotFound();
        }

        existingProperty.Title = property.Title;
        existingProperty.Description = property.Description;
        existingProperty.Location = property.Location;
        existingProperty.Price = property.Price;
        existingProperty.Bedrooms = property.Bedrooms;
        existingProperty.Bathrooms = property.Bathrooms;
        existingProperty.Latitude = property.Latitude;
        existingProperty.Longitude = property.Longitude;
        existingProperty.IsAvailable = property.IsAvailable;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/properties/{id}
    // Staff: Delete a property
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProperty(int id)
    {
        var property =
            await _context.Properties.FindAsync(id);

        if (property == null)
        {
            return NotFound();
        }

        _context.Properties.Remove(property);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH /api/properties/{id}/availability
    // Staff: Change property availability
    [HttpPatch("{id}/availability")]
    public async Task<IActionResult> UpdateAvailability(
        int id,
        [FromBody] bool isAvailable)
    {
        var property =
            await _context.Properties.FindAsync(id);

        if (property == null)
        {
            return NotFound();
        }

        property.IsAvailable = isAvailable;

        await _context.SaveChangesAsync();

        return Ok(property);
    }
}