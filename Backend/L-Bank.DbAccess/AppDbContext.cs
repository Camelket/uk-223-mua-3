using L_Bank_W_Backend.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess;

public class AppDbContext : DbContext
{
    public DbSet<Ledger> Ledgers { get; set; }
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
}
