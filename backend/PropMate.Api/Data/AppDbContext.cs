using Microsoft.EntityFrameworkCore;
using PropMate.Api.Models;

namespace PropMate.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<PropertyListing> PropertyListings => Set<PropertyListing>();

    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();

    public DbSet<ListingStatusHistory> ListingStatusHistories =>
        Set<ListingStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PropertyListing>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(x => x.Address)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Price)
                .HasPrecision(18, 2);

            entity.HasMany(x => x.Images)
                .WithOne(x => x.PropertyListing)
                .HasForeignKey(x => x.PropertyListingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.StatusHistory)
                .WithOne(x => x.PropertyListing)
                .HasForeignKey(x => x.PropertyListingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.Status);

            entity.HasIndex(x => x.City);

            entity.HasIndex(x => x.OwnerId);
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<ListingStatusHistory>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Reason)
                .HasMaxLength(500);
        });
    }
}