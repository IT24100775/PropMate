using System.ComponentModel.DataAnnotations;
using PropMate.Api.DTOs.Listings;
using PropMate.Api.Enums;

namespace PropMate.Api.Tests;

public class PropertyListingValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true);

        return results;
    }

    private static CreatePropertyListingDto CreateValidDto()
    {
        return new CreatePropertyListingDto
        {
            Title = "Modern Apartment in Colombo",
            Description =
                "A spacious modern apartment suitable for comfortable city living.",
            Purpose = ListingPurpose.Rent,
            PropertyType = PropertyType.Apartment,
            Price = 150000,
            Address = "123 Example Road",
            City = "Colombo",
            Latitude = 6.9271,
            Longitude = 79.8612,
            Bedrooms = 3,
            Bathrooms = 2
        };
    }

    [Fact]
    public void ValidListing_ShouldPassValidation()
    {
        var dto = CreateValidDto();

        var results = Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void TitleShorterThanFiveCharacters_ShouldFailValidation()
    {
        var dto = CreateValidDto();
        dto.Title = "Home";

        var results = Validate(dto);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(dto.Title)));
    }

    [Fact]
    public void DescriptionShorterThanTwentyCharacters_ShouldFailValidation()
    {
        var dto = CreateValidDto();
        dto.Description = "Too short";

        var results = Validate(dto);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(dto.Description)));
    }

    [Fact]
    public void ZeroPrice_ShouldFailValidation()
    {
        var dto = CreateValidDto();
        dto.Price = 0;

        var results = Validate(dto);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(dto.Price)));
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void LatitudeOutsideValidRange_ShouldFailValidation(double latitude)
    {
        var dto = CreateValidDto();
        dto.Latitude = latitude;

        var results = Validate(dto);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(dto.Latitude)));
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void LongitudeOutsideValidRange_ShouldFailValidation(double longitude)
    {
        var dto = CreateValidDto();
        dto.Longitude = longitude;

        var results = Validate(dto);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(dto.Longitude)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void InvalidBedroomCount_ShouldFailValidation(int bedrooms)
    {
        var dto = CreateValidDto();
        dto.Bedrooms = bedrooms;

        var results = Validate(dto);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(dto.Bedrooms)));
    }
}