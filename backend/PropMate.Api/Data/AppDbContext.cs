using Microsoft.EntityFrameworkCore;
using PropMate.Api.Models;

namespace PropMate.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PropertyListing> PropertyListings => Set<PropertyListing>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<ListingStatusHistory> ListingStatusHistories => Set<ListingStatusHistory>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PropertyListingVerification> PropertyListingVerifications => Set<PropertyListingVerification>();

    public DbSet<RentalApplication> RentalApplications => Set<RentalApplication>();
    public DbSet<RentalNegotiationOffer> RentalNegotiationOffers => Set<RentalNegotiationOffer>();
    public DbSet<RentalNegotiationMessage> RentalNegotiationMessages => Set<RentalNegotiationMessage>();
    public DbSet<RentalAgreement> RentalAgreements => Set<RentalAgreement>();
    public DbSet<PurchaseOffer> PurchaseOffers => Set<PurchaseOffer>();
    public DbSet<PurchaseNegotiationOffer> PurchaseNegotiationOffers => Set<PurchaseNegotiationOffer>();
    public DbSet<PurchaseNegotiationMessage> PurchaseNegotiationMessages => Set<PurchaseNegotiationMessage>();
    public DbSet<PurchaseAgreement> PurchaseAgreements => Set<PurchaseAgreement>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PropertyListing>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.Address).IsRequired().HasMaxLength(250);
            entity.Property(x => x.City).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.HasMany(x => x.Images).WithOne(x => x.PropertyListing).HasForeignKey(x => x.PropertyListingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.StatusHistory).WithOne(x => x.PropertyListing).HasForeignKey(x => x.PropertyListingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.City);
            entity.HasIndex(x => x.OwnerId);
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ImageUrl).IsRequired().HasMaxLength(1000);
        });

        modelBuilder.Entity<ListingStatusHistory>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Reason).HasMaxLength(500);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(255);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<PropertyListingVerification>()
            .HasOne(x => x.PropertyListing).WithMany(x => x.Verifications)
            .HasForeignKey(x => x.PropertyListingId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PropertyListingVerification>().HasIndex(x => x.PropertyListingId);

        modelBuilder.Entity<RentalApplication>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Employment).IsRequired().HasMaxLength(200);
            entity.Property(x => x.MonthlyIncome).HasPrecision(18, 2);
            entity.Property(x => x.Message).HasMaxLength(2000);
            entity.HasOne(x => x.PropertyListing).WithMany().HasForeignKey(x => x.PropertyListingId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.PropertyListingId);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<RentalNegotiationOffer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MonthlyRent).HasPrecision(18, 2);
            entity.Property(x => x.Conditions).HasMaxLength(2000);
            entity.HasOne(x => x.RentalApplication).WithMany(x => x.NegotiationOffers).HasForeignKey(x => x.RentalApplicationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProposedByUser).WithMany().HasForeignKey(x => x.ProposedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RentalApplicationId);
        });

        modelBuilder.Entity<RentalNegotiationMessage>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Message).IsRequired().HasMaxLength(2000);
            entity.HasOne(x => x.RentalApplication).WithMany(x => x.NegotiationMessages).HasForeignKey(x => x.RentalApplicationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SenderUser).WithMany().HasForeignKey(x => x.SenderUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RentalApplicationId);
        });

        modelBuilder.Entity<RentalAgreement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FinalMonthlyRent).HasPrecision(18, 2);
            entity.Property(x => x.Terms).HasMaxLength(4000);
            entity.Property(x => x.TenantObligation).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.OwnerObligation).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.PenaltyTerms).IsRequired().HasMaxLength(2000);
            entity.HasOne(x => x.RentalApplication).WithOne(x => x.Agreement).HasForeignKey<RentalAgreement>(x => x.RentalApplicationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.RentalApplicationId).IsUnique();
        });

        modelBuilder.Entity<PurchaseOffer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OfferAmount).HasPrecision(18, 2);
            entity.Property(x => x.Conditions).HasMaxLength(3000);
            entity.HasOne(x => x.PropertyListing).WithMany().HasForeignKey(x => x.PropertyListingId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Buyer).WithMany().HasForeignKey(x => x.BuyerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.PropertyListingId);
            entity.HasIndex(x => x.BuyerId);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<PurchaseNegotiationOffer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OfferAmount).HasPrecision(18, 2);
            entity.Property(x => x.Conditions).HasMaxLength(3000);
            entity.HasOne(x => x.PurchaseOffer).WithMany(x => x.NegotiationOffers).HasForeignKey(x => x.PurchaseOfferId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProposedByUser).WithMany().HasForeignKey(x => x.ProposedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.PurchaseOfferId);
        });

        modelBuilder.Entity<PurchaseNegotiationMessage>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Message).IsRequired().HasMaxLength(2000);
            entity.HasOne(x => x.PurchaseOffer).WithMany(x => x.NegotiationMessages).HasForeignKey(x => x.PurchaseOfferId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SenderUser).WithMany().HasForeignKey(x => x.SenderUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.PurchaseOfferId);
        });

        modelBuilder.Entity<PurchaseAgreement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FinalPurchasePrice).HasPrecision(18, 2);
            entity.Property(x => x.Conditions).HasMaxLength(4000);
            entity.Property(x => x.BuyerObligation).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.SellerObligation).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.PenaltyTerms).IsRequired().HasMaxLength(2000);
            entity.HasOne(x => x.PurchaseOffer).WithOne(x => x.Agreement).HasForeignKey<PurchaseAgreement>(x => x.PurchaseOfferId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.PurchaseOfferId).IsUnique();
        });

    }
}
