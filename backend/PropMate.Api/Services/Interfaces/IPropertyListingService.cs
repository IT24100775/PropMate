using PropMate.Api.DTOs.Listings;
using PropMate.Api.DTOs.Verification;

namespace PropMate.Api.Services.Interfaces;

public interface IPropertyListingService
{
    Task<PropertyListingResponseDto> CreateAsync(
        int ownerId,
        CreatePropertyListingDto dto);

    Task<PropertyListingResponseDto?> GetByIdAsync(int id);

    Task<IEnumerable<PropertyListingResponseDto>> GetByOwnerAsync(int ownerId);

    Task<PropertyListingResponseDto?> UpdateAsync(
        int id,
        int ownerId,
        UpdatePropertyListingDto dto);

    Task<bool> DeleteAsync(int id, int ownerId);

    Task<PropertyListingResponseDto?> SubmitAsync(
    int id,
    int ownerId);

    Task<PropertyListingResponseDto?> StartReviewAsync(
    int id,
    int adminUserId);

    Task<PropertyListingResponseDto?> ApproveAsync(
        int id,
        int adminUserId,
        string? reason);

    Task<PropertyListingResponseDto?> RejectAsync(
        int id,
        int adminUserId,
        string reason);

    Task<PropertyListingResponseDto?> RequestRevisionAsync(
        int id,
        int adminUserId,
        string reason);

    Task<PropertyListingResponseDto?> PublishAsync(
        int id,
        int adminUserId);

    Task<PropertyListingResponseDto?> UnpublishAsync(
        int id,
        int adminUserId);

    Task<PagedResultDto<PropertyListingResponseDto>> SearchAsync(
    PropertyListingQueryDto query);

    Task<PropertyListingResponseDto?> GetPublishedByIdAsync(int id);

    Task<PropertyVerificationReviewDto?> GetVerificationReviewAsync(int id);

}