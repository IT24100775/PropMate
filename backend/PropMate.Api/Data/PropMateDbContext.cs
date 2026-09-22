using Microsoft.EntityFrameworkCore;
using PropMate.Api.Models;

namespace PropMate.Api.Data;

public class PropMateDbContext : DbContext
{
    public PropMateDbContext(DbContextOptions<PropMateDbContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Favourite> Favourites => Set<Favourite>();
    public DbSet<ViewingSlot> ViewingSlots => Set<ViewingSlot>();
    public DbSet<ViewingBooking> ViewingBookings => Set<ViewingBooking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.Property)
            .WithMany()
            .HasForeignKey(f => f.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ViewingSlot>()
            .HasOne(v => v.Property)
            .WithMany()
            .HasForeignKey(v => v.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ViewingBooking>()
            .HasOne(b => b.ViewingSlot)
            .WithMany()
            .HasForeignKey(b => b.ViewingSlotId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ViewingBooking>()
            .HasIndex(b => b.ViewingSlotId)
            .IsUnique();
    }
}
