using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.Enums;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PropertiesController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/properties
    // Discovery endpoint - returns published property listings only
    [HttpGet]
    public async Task<IActionResult> GetProperties(
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? bedrooms,
        [FromQuery] int? bathrooms,
        [FromQuery] string? location,
        [FromQuery] bool? isAvailable)
    {
        var query = _context.PropertyListings
            .Where(p => p.Status == ListingStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();

            query = query.Where(p =>
                p.Title.ToLower().Contains(searchLower) ||
                p.City.ToLower().Contains(searchLower) ||
                p.Address.ToLower().Contains(searchLower));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (bedrooms.HasValue)
        {
            query = query.Where(p => p.Bedrooms >= bedrooms.Value);
        }

        if (bathrooms.HasValue)
        {
            query = query.Where(p => p.Bathrooms >= bathrooms.Value);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var locationLower = location.Trim().ToLower();

            query = query.Where(p =>
                p.City.ToLower().Contains(locationLower) ||
                p.Address.ToLower().Contains(locationLower));
        }

        // Published listings are the discoverable/available listings.
        // If false is requested, no published results are returned.
        if (isAvailable.HasValue && !isAvailable.Value)
        {
            query = query.Where(p => false);
        }

        var properties = await query
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Description,
                p.Price,
                p.Bedrooms,
                p.Bathrooms,

                // Preserve Component 2 API field names
                Location = string.IsNullOrWhiteSpace(p.City)
                    ? p.Address
                    : p.City,

                p.Latitude,
                p.Longitude,

                IsAvailable = true
            })
            .ToListAsync();

        return Ok(properties);
    }

    // GET /api/properties/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProperty(int id)
    {
        var property = await _context.PropertyListings
            .Where(p =>
                p.Id == id &&
                p.Status == ListingStatus.Published)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Description,
                p.Price,
                p.Bedrooms,
                p.Bathrooms,

                Location = string.IsNullOrWhiteSpace(p.City)
                    ? p.Address
                    : p.City,

                p.Latitude,
                p.Longitude,

                IsAvailable = true
            })
            .FirstOrDefaultAsync();

        if (property == null)
        {
            return NotFound(new
            {
                message = "Published property listing not found."
            });
        }

        return Ok(property);
    }

    // GET /api/properties/{id}/location
    [HttpGet("{id}/location")]
    public async Task<IActionResult> GetPropertyLocation(int id)
    {
        var locationData = await _context.PropertyListings
            .Where(p =>
                p.Id == id &&
                p.Status == ListingStatus.Published)
            .Select(p => new
            {
                Location = string.IsNullOrWhiteSpace(p.City)
                    ? p.Address
                    : p.City,

                p.Latitude,
                p.Longitude
            })
            .FirstOrDefaultAsync();

        if (locationData == null)
        {
            return NotFound(new
            {
                message = "Published property listing not found."
            });
        }

        return Ok(locationData);
    }

    // POST /api/properties/natural-search
    [HttpPost("natural-search")]
    public async Task<IActionResult> NaturalLanguageSearch(
        [FromBody] NaturalLanguageSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest("Search query is required.");
        }

        var search = request.Query.Trim().ToLower();

        var query = _context.PropertyListings
            .Where(p => p.Status == ListingStatus.Published)
            .AsQueryable();

        if (search.Contains("colombo"))
        {
            query = query.Where(p =>
                p.City.ToLower().Contains("colombo") ||
                p.Address.ToLower().Contains("colombo"));
        }

        if (search.Contains("kandy"))
        {
            query = query.Where(p =>
                p.City.ToLower().Contains("kandy") ||
                p.Address.ToLower().Contains("kandy"));
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

        var properties = await query
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Description,
                p.Price,
                p.Bedrooms,
                p.Bathrooms,

                Location = string.IsNullOrWhiteSpace(p.City)
                    ? p.Address
                    : p.City,

                p.Latitude,
                p.Longitude,

                IsAvailable = true
            })
            .ToListAsync();

        return Ok(properties);
    }
}

public class NaturalLanguageSearchRequest
{
    public string Query { get; set; } = string.Empty;
}