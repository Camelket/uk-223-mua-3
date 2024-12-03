using L_Bank_W_Backend.Core.Models;
using L_Bank.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Deposit>(e =>
        {
            e.HasKey(e => e.Id);

            e.HasOne(e => e.Ledger)
                .WithMany(e => e.Deposits)
                .HasForeignKey(e => e.LedgerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(e => e.Depositor)
                .WithMany()
                .HasForeignKey(e => e.DepositorId)
                .OnDelete(DeleteBehavior.Cascade);

            e.ToTable("Deposits");
        });

        modelBuilder.Entity<Ledger>(e =>
        {
            e.HasKey(e => e.Id);

            e.HasOne(e => e.User)
                .WithMany(e => e.Ledgers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            e.ToTable("Ledgers");
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(e => e.Id);

            e.Property(e => e.Role).HasConversion<string>();

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
