using PropMate.Api.DTOs.Verification;

namespace PropMate.Api.Services;

public interface IPropertyVerificationClient
{
    Task<PropertyVerificationResponse?> VerifyListingAsync(
        PropertyVerificationRequest request,
        CancellationToken cancellationToken = default);
}