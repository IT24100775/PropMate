using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.DTOs.Listings;
using PropMate.Api.Enums;
using PropMate.Api.Models;
using PropMate.Api.Services.Interfaces;
using System.Text.Json;
using PropMate.Api.DTOs.Verification;

namespace PropMate.Api.Services;

public class PropertyListingService : IPropertyListingService
{
    private readonly AppDbContext _context;
    private readonly IPropertyVerificationClient _verificationClient;

    public PropertyListingService(
        AppDbContext context,
        IPropertyVerificationClient verificationClient)
    {
        _context = context;
        _verificationClient = verificationClient;
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
            .Include(x => x.StatusHistory)
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

        // -------------------------------------------------
        // 1. Owner submits the listing
        // -------------------------------------------------

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

        // -------------------------------------------------
        // 2. Prepare real listing data for the AI service
        // -------------------------------------------------

        var verificationRequest =
            await BuildVerificationRequestAsync(listing);

        // -------------------------------------------------
        // 3. Call the Python verification agent
        // -------------------------------------------------

        var verificationResult =
            await _verificationClient.VerifyListingAsync(
                verificationRequest);

        // -------------------------------------------------
        // 4. Handle Python service failure safely
        // -------------------------------------------------

        if (verificationResult == null)
        {
            var failedVerification =
                new PropertyListingVerification
                {
                    PropertyListingId = listing.Id,
                    Recommendation = "ADMIN_REVIEW",
                    Confidence = 0,
                    RiskScore = 0,
                    ReasonsJson = JsonSerializer.Serialize(
                        new[]
                        {
                            "Automated verification service was unavailable. " +
                            "Manual administrator review is required."
                        }),
                    EvidenceJson = "[]",
                    RequiresHumanReview = true,
                    CreatedAt = DateTime.UtcNow
                };

            _context.PropertyListingVerifications.Add(
                failedVerification);

            listing.Status = ListingStatus.UnderReview;
            listing.UpdatedAt = DateTime.UtcNow;

            listing.StatusHistory.Add(new ListingStatusHistory
            {
                PreviousStatus = ListingStatus.Submitted,
                NewStatus = ListingStatus.UnderReview,
                Reason =
                    "Automated verification was unavailable. " +
                    "Listing sent for manual administrator review.",
                ChangedByUserId = ownerId,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return MapToDto(listing);
        }

        // -------------------------------------------------
        // 5. Store successful agent verification result
        // -------------------------------------------------

        var verification =
            new PropertyListingVerification
            {
                PropertyListingId = listing.Id,
                Recommendation = verificationResult.Recommendation,
                Confidence = verificationResult.Confidence,
                RiskScore = verificationResult.RiskScore,
                ReasonsJson = JsonSerializer.Serialize(
                    verificationResult.Reasons),
                EvidenceJson = JsonSerializer.Serialize(
                    verificationResult.Evidence),
                RequiresHumanReview = true,
                CreatedAt = DateTime.UtcNow
            };

        _context.PropertyListingVerifications.Add(
            verification);

        // -------------------------------------------------
        // 6. Move listing into human review
        // -------------------------------------------------

        listing.Status = ListingStatus.UnderReview;
        listing.UpdatedAt = DateTime.UtcNow;

        listing.StatusHistory.Add(new ListingStatusHistory
        {
            PreviousStatus = ListingStatus.Submitted,
            NewStatus = ListingStatus.UnderReview,
            Reason =
                $"Automated verification completed. " +
                $"Recommendation: {verificationResult.Recommendation}.",
            ChangedByUserId = ownerId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return MapToDto(listing);
    }

    public async Task<PropertyListingResponseDto?> StartReviewAsync(
    int id,
    int adminUserId)
    {
        var listing = await _context.PropertyListings
            .Include(x => x.Images)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (listing == null)
        {
            return null;
        }

        if (listing.Status != ListingStatus.Submitted)
        {
            throw new InvalidOperationException(
                "Only submitted listings can enter review.");
        }

        var previousStatus = listing.Status;

        listing.Status = ListingStatus.UnderReview;
        listing.UpdatedAt = DateTime.UtcNow;

        listing.StatusHistory.Add(new ListingStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = ListingStatus.UnderReview,
            Reason = "Listing moved to administrative review.",
            ChangedByUserId = adminUserId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return MapToDto(listing);
    }

    public async Task<PropertyListingResponseDto?> ApproveAsync(
    int id,
    int adminUserId,
    string? reason)
    {
        var listing = await _context.PropertyListings
            .Include(x => x.Images)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (listing == null)
        {
            return null;
        }

        if (listing.Status != ListingStatus.UnderReview)
        {
            throw new InvalidOperationException(
                "Only listings under review can be approved.");
        }

        var previousStatus = listing.Status;

        listing.Status = ListingStatus.Approved;
        listing.UpdatedAt = DateTime.UtcNow;

        listing.StatusHistory.Add(new ListingStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = ListingStatus.Approved,
            Reason = reason ?? "Listing approved by administrator.",
            ChangedByUserId = adminUserId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return MapToDto(listing);
    }

    public async Task<PropertyListingResponseDto?> RejectAsync(
    int id,
    int adminUserId,
    string reason)
    {
        var listing = await _context.PropertyListings
            .Include(x => x.Images)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (listing == null)
        {
            return null;
        }

        if (listing.Status != ListingStatus.UnderReview)
        {
            throw new InvalidOperationException(
                "Only listings under review can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException(
                "A rejection reason is required.");
        }

        var previousStatus = listing.Status;

        listing.Status = ListingStatus.Rejected;
        listing.UpdatedAt = DateTime.UtcNow;

        listing.StatusHistory.Add(new ListingStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = ListingStatus.Rejected,
            Reason = reason.Trim(),
            ChangedByUserId = adminUserId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return MapToDto(listing);
    }

    public async Task<PropertyListingResponseDto?> RequestRevisionAsync(
    int id,
    int adminUserId,
    string reason)
    {
        var listing = await _context.PropertyListings
            .Include(x => x.Images)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (listing == null)
        {
            return null;
        }

        if (listing.Status != ListingStatus.UnderReview)
        {
            throw new InvalidOperationException(
                "Only listings under review can be sent for revision.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException(
                "A revision reason is required.");
        }

        var previousStatus = listing.Status;

        listing.Status = ListingStatus.RevisionRequired;
        listing.UpdatedAt = DateTime.UtcNow;

        listing.StatusHistory.Add(new ListingStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = ListingStatus.RevisionRequired,
            Reason = reason.Trim(),
            ChangedByUserId = adminUserId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return MapToDto(listing);
    }

    public async Task<PropertyListingResponseDto?> PublishAsync(
    int id,
    int adminUserId)
    {
        var listing = await _context.PropertyListings
            .Include(x => x.Images)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (listing == null)
        {
            return null;
        }

        if (listing.Status != ListingStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved listings can be published.");
        }

        var previousStatus = listing.Status;

        listing.Status = ListingStatus.Published;
        listing.UpdatedAt = DateTime.UtcNow;

        listing.StatusHistory.Add(new ListingStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = ListingStatus.Published,
            Reason = "Listing published by administrator.",
            ChangedByUserId = adminUserId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return MapToDto(listing);
    }

    public async Task<PropertyListingResponseDto?> UnpublishAsync(
    int id,
    int adminUserId)
    {
        var listing = await _context.PropertyListings
            .Include(x => x.Images)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (listing == null)
        {
            return null;
        }

        if (listing.Status != ListingStatus.Published)
        {
            throw new InvalidOperationException(
                "Only published listings can be unpublished.");
        }

        var previousStatus = listing.Status;

        listing.Status = ListingStatus.Unpublished;
        listing.UpdatedAt = DateTime.UtcNow;

        listing.StatusHistory.Add(new ListingStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = ListingStatus.Unpublished,
            Reason = "Listing unpublished by administrator.",
            ChangedByUserId = adminUserId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return MapToDto(listing);
    }

    public async Task<PagedResultDto<PropertyListingResponseDto>> SearchAsync(
    PropertyListingQueryDto query)
    {
        var listings = _context.PropertyListings
            .AsNoTracking()
            .Include(x => x.Images)
            .AsQueryable();

        // Only public/published listings should appear in public search
        listings = listings.Where(x =>
            x.Status == ListingStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();

            listings = listings.Where(x =>
                x.Title.ToLower().Contains(search) ||
                x.Description.ToLower().Contains(search) ||
                x.City.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var city = query.City.Trim().ToLower();

            listings = listings.Where(x =>
                x.City.ToLower() == city);
        }

        if (query.Purpose.HasValue)
        {
            listings = listings.Where(x =>
                x.Purpose == query.Purpose.Value);
        }

        if (query.PropertyType.HasValue)
        {
            listings = listings.Where(x =>
                x.PropertyType == query.PropertyType.Value);
        }

        if (query.MinPrice.HasValue)
        {
            listings = listings.Where(x =>
                x.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            listings = listings.Where(x =>
                x.Price <= query.MaxPrice.Value);
        }

        var totalCount = await listings.CountAsync();

        var sortBy = query.SortBy.ToLower();
        var sortOrder = query.SortOrder.ToLower();

        listings = (sortBy, sortOrder) switch
        {
            ("price", "asc") =>
                listings.OrderBy(x => x.Price),

            ("price", "desc") =>
                listings.OrderByDescending(x => x.Price),

            ("title", "asc") =>
                listings.OrderBy(x => x.Title),

            ("title", "desc") =>
                listings.OrderByDescending(x => x.Title),

            ("createdat", "asc") =>
                listings.OrderBy(x => x.CreatedAt),

            _ =>
                listings.OrderByDescending(x => x.CreatedAt)
        };

        var items = await listings
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResultDto<PropertyListingResponseDto>
        {
            Items = items.Select(MapToDto),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)query.PageSize)
        };
    }

    public async Task<PropertyListingResponseDto?> GetPublishedByIdAsync(int id)
    {
        var listing = await _context.PropertyListings
            .AsNoTracking()
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Status == ListingStatus.Published);

        return listing == null ? null : MapToDto(listing);
    }

    private async Task<PropertyVerificationRequest> BuildVerificationRequestAsync(
    PropertyListing listing)
    {
        var comparableListings = await _context.PropertyListings
            .AsNoTracking()
            .Where(x =>
                x.Id != listing.Id &&
                x.Status == ListingStatus.Published &&
                x.City.ToLower() == listing.City.ToLower() &&
                x.PropertyType == listing.PropertyType &&
                x.Purpose == listing.Purpose)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToListAsync();

        var duplicateCandidates = await _context.PropertyListings
            .AsNoTracking()
            .Where(x =>
                x.Id != listing.Id &&
                x.Address.ToLower() == listing.Address.ToLower())
            .Take(5)
            .ToListAsync();

        var owner = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == listing.OwnerId);

        return new PropertyVerificationRequest
        {
            ListingId = listing.Id,
            OwnerId = listing.OwnerId,
            Title = listing.Title,
            Description = listing.Description,
            Purpose = listing.Purpose.ToString(),
            PropertyType = listing.PropertyType.ToString(),
            Price = listing.Price,
            Address = listing.Address,
            City = listing.City,
            Bedrooms = listing.Bedrooms,
            Bathrooms = listing.Bathrooms,

            Images = listing.Images
                .Select(x => new VerificationImageRequest
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                })
                .ToList(),

            // Temporary until owner verification is implemented
            OwnerVerified = owner?.IsVerified ?? false,

            DuplicateCandidates = duplicateCandidates
                .Select(x => new DuplicateCandidateRequest
                {
                    ListingId = x.Id,
                    Title = x.Title,
                    Address = x.Address
                })
                .ToList(),

            ComparableProperties = comparableListings
                .Select(x => new ComparablePropertyRequest
                {
                    ListingId = x.Id,
                    Price = x.Price,
                    City = x.City,
                    PropertyType = x.PropertyType.ToString()
                })
                .ToList()
        };
    }

    public async Task<PropertyVerificationReviewDto?> GetVerificationReviewAsync(
        int id)
    {
        var listing = await _context.PropertyListings
            .AsNoTracking()
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (listing == null)
        {
            return null;
        }

        var verification = await _context.PropertyListingVerifications
            .AsNoTracking()
            .Where(x => x.PropertyListingId == id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (verification == null)
        {
            return null;
        }

        var reasons =
            JsonSerializer.Deserialize<List<string>>(
                verification.ReasonsJson)
            ?? [];

        var evidence =
            JsonSerializer.Deserialize<List<VerificationEvidenceResponse>>(
                verification.EvidenceJson)
            ?? [];

        return new PropertyVerificationReviewDto
        {
            ListingId = listing.Id,
            OwnerId = listing.OwnerId,
            Title = listing.Title,
            Description = listing.Description,
            Purpose = listing.Purpose.ToString(),
            PropertyType = listing.PropertyType.ToString(),
            Price = listing.Price,
            Address = listing.Address,
            City = listing.City,
            Bedrooms = listing.Bedrooms,
            Bathrooms = listing.Bathrooms,
            Status = listing.Status.ToString(),

            ImageUrls = listing.Images
                .Select(x => x.ImageUrl)
                .ToList(),

            Recommendation = verification.Recommendation,
            Confidence = verification.Confidence,
            RiskScore = verification.RiskScore,
            Reasons = reasons,
            Evidence = evidence,
            RequiresHumanReview = verification.RequiresHumanReview,
            VerifiedAt = verification.CreatedAt
        };
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