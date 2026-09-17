using Microsoft.AspNetCore.Mvc;
using PropMate.Api.DTOs.Listings;
using PropMate.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

    [Authorize(Roles = "OwnerAgent")]
    [HttpGet("owner")]
    public async Task<ActionResult<IEnumerable<PropertyListingResponseDto>>>
        GetOwnerListings()
    {
        var ownerId = GetCurrentUserId();

        var listings = await _service.GetByOwnerAsync(ownerId);

        return Ok(listings);
    }

    [Authorize(Roles = "OwnerAgent")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PropertyListingResponseDto>> Update(
        int id,
        [FromBody] UpdatePropertyListingDto dto)
    {
        var ownerId = GetCurrentUserId();;

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

    [Authorize(Roles = "OwnerAgent")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ownerId = GetCurrentUserId();;

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

    [Authorize(Roles = "OwnerAgent")]
    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<PropertyListingResponseDto>> Submit(int id)
    {
        var ownerId = GetCurrentUserId();;

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

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/review")]
    public async Task<ActionResult<PropertyListingResponseDto>> StartReview(int id)
    {
        var adminUserId = GetCurrentUserId();

        try
        {
            var listing = await _service.StartReviewAsync(id, adminUserId);

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

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<PropertyListingResponseDto>> Approve(
        int id,
        [FromBody] AdminListingDecisionDto? dto)
    {
        var adminUserId = GetCurrentUserId();

        try
        {
            var listing = await _service.ApproveAsync(
                id,
                adminUserId,
                dto?.Reason);

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

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<PropertyListingResponseDto>> Reject(
        int id,
        [FromBody] AdminListingDecisionDto dto)
    {
        var adminUserId = GetCurrentUserId();

        try
        {
            var listing = await _service.RejectAsync(
                id,
                adminUserId,
                dto.Reason);

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

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/request-revision")]
    public async Task<ActionResult<PropertyListingResponseDto>> RequestRevision(
        int id,
        [FromBody] AdminListingDecisionDto dto)
    {
        var adminUserId = GetCurrentUserId();

        try
        {
            var listing = await _service.RequestRevisionAsync(
                id,
                adminUserId,
                dto.Reason);

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

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult<PropertyListingResponseDto>> Publish(int id)
    {
        var adminUserId = GetCurrentUserId();

        try
        {
            var listing = await _service.PublishAsync(id, adminUserId);

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

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/unpublish")]
    public async Task<ActionResult<PropertyListingResponseDto>> Unpublish(int id)
    {
        var adminUserId = GetCurrentUserId();

        try
        {
            var listing = await _service.UnpublishAsync(id, adminUserId);

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

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<PropertyListingResponseDto>>> Search(
        [FromQuery] PropertyListingQueryDto query)
    {
        if (query.MinPrice.HasValue &&
            query.MaxPrice.HasValue &&
            query.MinPrice > query.MaxPrice)
        {
            return BadRequest(new
            {
                message = "Minimum price cannot be greater than maximum price."
            });
        }

        var result = await _service.SearchAsync(query);

        return Ok(result);
    }

    [Authorize(Roles = "OwnerAgent")]
    [HttpPost]
    public async Task<ActionResult<PropertyListingResponseDto>> Create(
        [FromBody] CreatePropertyListingDto dto)
    {
        var ownerId = GetCurrentUserId();

        var listing = await _service.CreateAsync(ownerId, dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = listing.Id },
            listing);
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var id))
        {
            throw new UnauthorizedAccessException(
                "User ID could not be determined.");
        }

        return id;
    }

}