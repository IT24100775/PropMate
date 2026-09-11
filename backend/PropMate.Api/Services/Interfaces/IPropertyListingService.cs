using PropMate.Api.DTOs.Listings;

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
}