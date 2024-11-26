using System;
using L_Bank_W_Backend.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace L_Bank_W_Backend.DbAccess;

public class AppDbContext : DbContext
{
    public DbSet<Ledger> Ledgers { get; set; }
    public DbSet<User> Users { get; set; }

    public AppDbContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=localhost; Database=l_bank_backend; Integrated Security = True;"
        );
    }
}
