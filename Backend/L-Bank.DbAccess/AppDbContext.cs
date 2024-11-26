using L_Bank_W_Backend.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ledger>(e =>
        {
            e.HasKey(e => e.Id);

            e.HasOne(e => e.User)
                .WithMany(e => e.Ledgers)
                .HasForeignKey(e => e.UserId);

            e.ToTable("Ledgers");
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(e => e.Id);

            e.HasDiscriminator(e => e.Role).HasValue(Roles.Administrators).HasValue(Roles.Users);

            e.ToTable("Users");
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(e => e.Id);

            e.HasOne(e => e.Source)
                .WithMany()
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(e => e.Destination)
                .WithMany()
                .HasForeignKey(e => e.DestinationId)
                .OnDelete(DeleteBehavior.NoAction);

            e.ToTable("Bookings");
        });
    }
}
