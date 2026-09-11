using Microsoft.AspNetCore.Mvc;
using PropMate.Api.DTOs.Listings;
using PropMate.Api.Services.Interfaces;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyListingsController : ControllerBase
{
    private readonly IPropertyListingService _service;

    public PropertyListingsController(IPropertyListingService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<PropertyListingResponseDto>> Create(
        [FromBody] CreatePropertyListingDto dto)
    {
        // Temporary owner ID until JWT authentication is added
        var ownerId = 1;

        var listing = await _service.CreateAsync(ownerId, dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = listing.Id },
            listing
        );
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PropertyListingResponseDto>> GetById(int id)
    {
        var listing = await _service.GetByIdAsync(id);

        if (listing == null)
        {
            return NotFound(new
            {
                message = "Property listing not found."
            });
        }

        return Ok(listing);
    }

    [HttpGet("owner")]
    public async Task<ActionResult<IEnumerable<PropertyListingResponseDto>>> GetOwnerListings()
    {
        // Temporary owner ID until JWT authentication is added
        var ownerId = 1;

        var listings = await _service.GetByOwnerAsync(ownerId);

        return Ok(listings);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PropertyListingResponseDto>> Update(
        int id,
        [FromBody] UpdatePropertyListingDto dto)
    {
        var ownerId = 1;

        try
        {
            var listing = await _service.UpdateAsync(id, ownerId, dto);

            if (listing == null)
            {
                return NotFound(new
                {
                    message = "Property listing not found."
                });
            }

            return Ok(listing);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ownerId = 1;

        try
        {
            var deleted = await _service.DeleteAsync(id, ownerId);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Property listing not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<PropertyListingResponseDto>> Submit(int id)
    {
        // Temporary until JWT authentication is implemented
        var ownerId = 1;

        try
        {
            var listing = await _service.SubmitAsync(id, ownerId);

            if (listing == null)
            {
                return NotFound(new
                {
                    message = "Property listing not found."
                });
            }

            return Ok(listing);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

}