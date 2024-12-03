using L_Bank_W_Backend.Core.Models;
using L_Bank.Core.Helper;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess;

public static class SeedData
{
    public static void Seed(DbContext context)
    {
        var userCount = context.Set<User>().Count();
        var ledgerCount = context.Set<Ledger>().Count();

        if (userCount < 1)
        {
            context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Users ON");
            foreach (User user in UserSeed)
            {
                context.Add(user);
            }
            context.SaveChanges();
            context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Users OFF");
        }

        if (ledgerCount < 1)
        {
            foreach (Ledger ledger in LedgerSeed)
            {
                context.Add(ledger);
            }
            context.SaveChanges();
        }
    }

    public static List<User> UserSeed =
    [
        new()
        {
            Id = 1,
            Username = "Admin",
            PasswordHash = PasswordHelper.HashAndSaltPassword("adminpass"),
            Role = Roles.Admin,
        },
        new()
        {
            Id = 2,
            Username = "User",
            PasswordHash = PasswordHelper.HashAndSaltPassword("userpass"),
            Role = Roles.User,
        },
    ];
    public static List<Ledger> LedgerSeed =
    [
        new()
        {
            Name = "Manitu AG",
            Balance = 1000,
            UserId = 1,
        },
        new()
        {
            Name = "Chrysalkis GmbH",
            Balance = 2000,
            UserId = 2,
        },
        new()
        {
            Name = "Smith & Co KG",
            Balance = 3000,
            UserId = 2,
        },
    ];
}
