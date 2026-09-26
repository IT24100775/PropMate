using System.Net.Http.Json;
using PropMate.Api.DTOs.Verification;

namespace PropMate.Api.Services;

public class PropertyVerificationClient : IPropertyVerificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PropertyVerificationClient> _logger;

    public PropertyVerificationClient(
        HttpClient httpClient,
        ILogger<PropertyVerificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PropertyVerificationResponse?> VerifyListingAsync(
        PropertyVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/verify-listing",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                _logger.LogWarning(
                    "Property verification service returned status code {StatusCode}. Response: {Response}",
                    response.StatusCode,
                    errorContent);

                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<PropertyVerificationResponse>(
                    cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Unable to connect to the property verification service.");

            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Property verification service request timed out.");

            return null;
        }
    }
}