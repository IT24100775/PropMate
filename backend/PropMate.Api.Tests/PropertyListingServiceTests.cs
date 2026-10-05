using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.DTOs.Verification;
using PropMate.Api.Enums;
using PropMate.Api.Models;
using PropMate.Api.Services;

namespace PropMate.Api.Tests;

public class PropertyListingServiceTests
{
    private sealed class FakeVerificationClient : IPropertyVerificationClient
    {
        public Task<PropertyVerificationResponse?> VerifyListingAsync(
            PropertyVerificationRequest request,
            CancellationToken cancellationToken = default)
        {
            // Safe-failure path: verification service unavailable.
            return Task.FromResult<PropertyVerificationResponse?>(null);
        }
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static PropertyListingService CreateService(AppDbContext context)
    {
        return new PropertyListingService(
            context,
            new FakeVerificationClient());
    }

    private static PropertyListing CreateListing(
        ListingStatus status,
        int ownerId = 100)
    {
        return new PropertyListing
        {
            OwnerId = ownerId,
            Title = "Modern Apartment in Colombo",
            Description =
                "A spacious modern apartment suitable for comfortable city living.",
            Purpose = ListingPurpose.Rent,
            PropertyType = PropertyType.Apartment,
            Price = 150000,
            Address = "123 Example Road",
            City = "Colombo",
            Bedrooms = 3,
            Bathrooms = 2,
            Status = status
        };
    }

    [Fact]
    public async Task SubmitDraftWithoutImage_ShouldThrowException()
    {
        await using var context = CreateContext();

        var listing = CreateListing(ListingStatus.Draft);
        context.PropertyListings.Add(listing);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(listing.Id, listing.OwnerId));

        Assert.Contains("At least one property image", exception.Message);
    }

    [Fact]
    public async Task SubmitApprovedListing_ShouldThrowException()
    {
        await using var context = CreateContext();

        var listing = CreateListing(ListingStatus.Approved);
        listing.Images.Add(new PropertyImage
        {
            ImageUrl = "https://example.com/property.jpg"
        });

        context.PropertyListings.Add(listing);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(listing.Id, listing.OwnerId));

        Assert.Contains(
            "Only draft or revision-required listings",
            exception.Message);
    }

    [Fact]
    public async Task ApproveUnderReviewListing_ShouldChangeStatusToApproved()
    {
        await using var context = CreateContext();

        var listing = CreateListing(ListingStatus.UnderReview);
        context.PropertyListings.Add(listing);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ApproveAsync(
            listing.Id,
            adminUserId: 1,
            reason: "Verification completed.");

        Assert.NotNull(result);
        Assert.Equal(ListingStatus.Approved, listing.Status);
        Assert.Single(listing.StatusHistory);
        Assert.Equal(
            ListingStatus.Approved,
            listing.StatusHistory.Single().NewStatus);
    }

    [Fact]
    public async Task ApproveDraftListing_ShouldThrowException()
    {
        await using var context = CreateContext();

        var listing = CreateListing(ListingStatus.Draft);
        context.PropertyListings.Add(listing);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ApproveAsync(
                listing.Id,
                adminUserId: 1,
                reason: null));

        Assert.Contains(
            "Only listings under review can be approved",
            exception.Message);
    }

    [Fact]
    public async Task RejectWithoutReason_ShouldThrowException()
    {
        await using var context = CreateContext();

        var listing = CreateListing(ListingStatus.UnderReview);
        context.PropertyListings.Add(listing);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RejectAsync(
                listing.Id,
                adminUserId: 1,
                reason: "   "));

        Assert.Contains(
            "A rejection reason is required",
            exception.Message);
    }

    [Fact]
    public async Task PublishApprovedListing_ShouldChangeStatusToPublished()
    {
        await using var context = CreateContext();

        var listing = CreateListing(ListingStatus.Approved);
        context.PropertyListings.Add(listing);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.PublishAsync(
            listing.Id,
            adminUserId: 1);

        Assert.NotNull(result);
        Assert.Equal(ListingStatus.Published, listing.Status);
        Assert.Single(listing.StatusHistory);
        Assert.Equal(
            ListingStatus.Published,
            listing.StatusHistory.Single().NewStatus);
    }

    [Fact]
    public async Task PublishUnderReviewListing_ShouldThrowException()
    {
        await using var context = CreateContext();

        var listing = CreateListing(ListingStatus.UnderReview);
        context.PropertyListings.Add(listing);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishAsync(
                listing.Id,
                adminUserId: 1));

        Assert.Contains(
            "Only approved listings can be published",
            exception.Message);
    }
}