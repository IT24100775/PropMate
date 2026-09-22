using Microsoft.EntityFrameworkCore;
using PropMate.Api.Models;

namespace PropMate.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(PropMateDbContext context)
    {
        // Avoid inserting duplicate sample properties if data already exists
        if (!await context.Properties.AnyAsync())
        {
            context.Properties.AddRange(
                new Property
                {
                    Title = "Modern Luxury Apartment in Colombo 03",
                    Description = "Spacious 2-bedroom luxury apartment with ocean views, modern kitchen, and rooftop pool.",
                    Price = 45000000m,
                    Bedrooms = 2,
                    Bathrooms = 2,
                    Location = "Colombo 03",
                    Latitude = 6.9056,
                    Longitude = 79.8514,
                    IsAvailable = true
                },
                new Property
                {
                    Title = "Scenic Hillside Villa in Kandy",
                    Description = "Charming 4-bedroom family house situated in Kandy hills with scenic mountain views and spacious garden.",
                    Price = 38000000m,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Location = "Kandy",
                    Latitude = 7.2906,
                    Longitude = 80.6337,
                    IsAvailable = true
                },
                new Property
                {
                    Title = "Spacious Colonial Family House in Colombo 07",
                    Description = "Elegant 3-bedroom residential house with private garden, large living spaces, and secure parking.",
                    Price = 75000000m,
                    Bedrooms = 3,
                    Bathrooms = 3,
                    Location = "Colombo 07",
                    Latitude = 6.9098,
                    Longitude = 79.8698,
                    IsAvailable = true
                }
            );

            await context.SaveChangesAsync();
        }
    }
}
