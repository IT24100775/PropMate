using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.DTOs.Listings;
using PropMate.Api.Enums;
using PropMate.Api.Models;
using PropMate.Api.Services.Interfaces;

namespace PropMate.Api.Services;

public class PropertyListingService : IPropertyListingService
{
    private readonly AppDbContext _context;

    public PropertyListingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyListingResponseDto> CreateAsync(
        int ownerId,
        CreatePropertyListingDto dto)
    {
        var listing = new PropertyListing
        {
            OwnerId = ownerId,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Purpose = dto.Purpose,
            PropertyType = dto.PropertyType,
            Price = dto.Price,
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            Status = ListingStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var imageUrl in dto.ImageUrls)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                listing.Images.Add(new PropertyImage
                {
                    ImageUrl = imageUrl.Trim()
                });
            }
        }

        _context.PropertyListings.Add(listing);

        await _context.SaveChangesAsync();

        return MapToDto(listing);
    }

    public async Task<PropertyListingResponseDto?> GetByIdAsync(int id)
    {
        var listing = await _context.PropertyListings
            .AsNoTracking()
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

        return listing == null
            ? null
            : MapToDto(listing);
    }

    public async Task<IEnumerable<PropertyListingResponseDto>> GetByOwnerAsync(
        int ownerId)
    {
        var listings = await _context.PropertyListings
            .AsNoTracking()
            .Include(x => x.Images)
            .Where(x => x.OwnerId == ownerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return listings.Select(MapToDto);
    }

    public async Task<PropertyListingResponseDto?> UpdateAsync(
        int id,
        int ownerId,
        UpdatePropertyListingDto dto)
    {
        var listing = await _context.PropertyListings
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.OwnerId == ownerId);

        if (listing == null)
        {
            return null;
        }

        if (listing.Status != ListingStatus.Draft &&
            listing.Status != ListingStatus.RevisionRequired)
        {
            throw new InvalidOperationException(
                "Only draft or revision-required listings can be edited.");
        }

        listing.Title = dto.Title.Trim();
        listing.Description = dto.Description.Trim();
        listing.Purpose = dto.Purpose;
        listing.PropertyType = dto.PropertyType;
        listing.Price = dto.Price;
        listing.Address = dto.Address.Trim();
        listing.City = dto.City.Trim();
        listing.Bedrooms = dto.Bedrooms;
        listing.Bathrooms = dto.Bathrooms;
        listing.UpdatedAt = DateTime.UtcNow;

        _context.PropertyImages.RemoveRange(listing.Images);

        listing.Images = dto.ImageUrls
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => new PropertyImage
            {
                ImageUrl = x.Trim()
            })
            .ToList();

        await _context.SaveChangesAsync();

        return MapToDto(listing);
    }

    public async Task<bool> DeleteAsync(int id, int ownerId)
    {
        var listing = await _context.PropertyListings
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.OwnerId == ownerId);

        if (listing == null)
        {
            return false;
        }

        if (listing.Status != ListingStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft listings can be deleted.");
        }

        _context.PropertyListings.Remove(listing);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PropertyListingResponseDto?> SubmitAsync(
    int id,
    int ownerId)
    {
        var listing = await _context.PropertyListings
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.OwnerId == ownerId);

        if (listing == null)
        {
            return null;
        }

        if (listing.Status != ListingStatus.Draft &&
            listing.Status != ListingStatus.RevisionRequired)
        {
            throw new InvalidOperationException(
                "Only draft or revision-required listings can be submitted.");
        }

        if (listing.Images.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one property image is required before submission.");
        }

        var previousStatus = listing.Status;

        listing.Status = ListingStatus.Submitted;
        listing.UpdatedAt = DateTime.UtcNow;

        listing.StatusHistory.Add(new ListingStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = ListingStatus.Submitted,
            Reason = "Listing submitted by owner for verification.",
            ChangedByUserId = ownerId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return MapToDto(listing);
    }

    private static PropertyListingResponseDto MapToDto(
        PropertyListing listing)
    {
        return new PropertyListingResponseDto
        {
            Id = listing.Id,
            OwnerId = listing.OwnerId,
            Title = listing.Title,
            Description = listing.Description,
            Purpose = listing.Purpose,
            PropertyType = listing.PropertyType,
            Price = listing.Price,
            Address = listing.Address,
            City = listing.City,
            Bedrooms = listing.Bedrooms,
            Bathrooms = listing.Bathrooms,
            Status = listing.Status,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt,
            ImageUrls = listing.Images
                .Select(x => x.ImageUrl)
                .ToList()
        };
    }
}