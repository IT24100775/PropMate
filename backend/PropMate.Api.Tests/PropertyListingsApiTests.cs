using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PropMate.Api.Tests;

public class PropertyListingsApiTests
{
    private static WebApplicationFactory<Program> CreateFactory()
    {
        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            "ThisIsATestJwtKeyThatIsLongEnoughForTesting123456");

        Environment.SetEnvironmentVariable(
            "Jwt__Issuer",
            "PropMate.Tests");

        Environment.SetEnvironmentVariable(
            "Jwt__Audience",
            "PropMate.Tests");

        Environment.SetEnvironmentVariable(
            "PropertyVerificationService__BaseUrl",
            "http://localhost:8001");

        Environment.SetEnvironmentVariable(
            "AgenticAiService__BaseUrl",
            "http://localhost:8002");

        Environment.SetEnvironmentVariable(
            "PropertyManager__Email",
            null);

        Environment.SetEnvironmentVariable(
            "PropertyManager__Password",
            null);

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Environment", "Testing");
            });
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnSuccess()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OwnerListings_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/propertylistings/owner");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task AdminListings_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/propertylistings/admin");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}