using PropMate.Api.Enums;
using PropMate.Api.Models;

namespace PropMate.Api.Data;

public static class DbInitializer
{
    public static void SeedDemoData(AppDbContext context)
    {
        // Check if properties already exist
        if (context.PropertyListings.Any())
        {
            return;
        }

        var p1 = new PropertyListing
        {
            OwnerId = 2,
            Title = "Greenfield Modern Villa",
            Description = "Spacious 3-bedroom modern house with a private garden, modular kitchen, and secure parking in a quiet neighborhood.",
            Purpose = ListingPurpose.Rent,
            PropertyType = PropertyType.House,
            Price = 120000m,
            Address = "142 Greenfield Park",
            City = "Colombo 05",
            Bedrooms = 3,
            Bathrooms = 2,
            Status = ListingStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10),
            Images = new List<PropertyImage>
            {
                new() { ImageUrl = "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?w=800&auto=format&fit=crop", IsPrimary = true },
                new() { ImageUrl = "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=800&auto=format&fit=crop", IsPrimary = false }
            }
        };

        var p2 = new PropertyListing
        {
            OwnerId = 2,
            Title = "Skyline Luxury Apartment",
            Description = "High-floor 2-bedroom luxury apartment featuring panoramic sea views, 24/7 security, swimming pool, and gym access.",
            Purpose = ListingPurpose.Rent,
            PropertyType = PropertyType.Apartment,
            Price = 180000m,
            Address = "Tower B, Marine Drive",
            City = "Colombo 03",
            Bedrooms = 2,
            Bathrooms = 2,
            Status = ListingStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            UpdatedAt = DateTime.UtcNow.AddDays(-7),
            Images = new List<PropertyImage>
            {
                new() { ImageUrl = "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800&auto=format&fit=crop", IsPrimary = true },
                new() { ImageUrl = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&auto=format&fit=crop", IsPrimary = false }
            }
        };

        var p3 = new PropertyListing
        {
            OwnerId = 2,
            Title = "Royal Palms Country Residence",
            Description = "Charming 4-bedroom family home with serene surroundings, expansive lawn, and modern solar hot water system.",
            Purpose = ListingPurpose.Rent,
            PropertyType = PropertyType.House,
            Price = 95000m,
            Address = "88 Royal Palm Way",
            City = "Kandy",
            Bedrooms = 4,
            Bathrooms = 3,
            Status = ListingStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-5),
            Images = new List<PropertyImage>
            {
                new() { ImageUrl = "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800&auto=format&fit=crop", IsPrimary = true },
                new() { ImageUrl = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800&auto=format&fit=crop", IsPrimary = false }
            }
        };

        context.PropertyListings.AddRange(p1, p2, p3);
        context.SaveChanges();

        // Seed a demo completed rental agreement for Tenant 1 on Property 1
        var rentalApp = new RentalApplication
        {
            PropertyListingId = p1.Id,
            TenantId = 1,
            Employment = "Senior Software Engineer",
            MonthlyIncome = 350000m,
            Occupants = 2,
            PreferredMoveInDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            DurationMonths = 12,
            Message = "Looking for a long-term lease for my family.",
            Status = TransactionSubmissionStatus.Accepted,
            NegotiationStatus = NegotiationStatus.ClosedAccepted,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-28)
        };
        context.RentalApplications.Add(rentalApp);
        context.SaveChanges();

        var agreement = new RentalAgreement
        {
            RentalApplicationId = rentalApp.Id,
            PropertyListingId = p1.Id,
            OwnerId = 2,
            TenantId = 1,
            FinalMonthlyRent = 120000m,
            MoveInDate = rentalApp.PreferredMoveInDate,
            DurationMonths = 12,
            Terms = "Standard residential lease agreement.",
            TenantObligation = "Pay monthly rent by the 5th day and maintain property.",
            OwnerObligation = "Maintain structural components and address maintenance promptly.",
            PenaltyTerms = "5% late payment penalty applies after 7 days.",
            BuyerConfirmed = true,
            BuyerConfirmedAt = DateTime.UtcNow.AddDays(-28),
            SellerConfirmed = true,
            SellerConfirmedAt = DateTime.UtcNow.AddDays(-28),
            Status = AgreementStatus.Completed,
            GeneratedAt = DateTime.UtcNow.AddDays(-29),
            CompletedAt = DateTime.UtcNow.AddDays(-28)
        };
        context.RentalAgreements.Add(agreement);
        context.SaveChanges();
    }
}
