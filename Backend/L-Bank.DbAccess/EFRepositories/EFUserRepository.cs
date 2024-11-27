using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.Interfaces;
using L_Bank.Core.Helper;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFUserRepository(AppDbContext context) : IEFUserRepository
{
    private readonly AppDbContext context = context;

    public async Task<User?> GetByUsername(string username)
    {
        return await context
            .Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task<User?> GetOne(int id, bool includeLedgers = false)
    {
        if (includeLedgers)
        {
            return await context
                .Set<User>()
                .Include(x => x.Ledgers)
                .AsNoTracking()
                .FirstAsync(x => x.Id == id);
        }
        else
        {
            return await context.Set<User>().AsNoTracking().FirstAsync(x => x.Id == id);
        }
    }

    public async Task<User> Save(User user)
    {
        var SavedUser = await context.AddAsync(user);
        await context.SaveChangesAsync();
        return SavedUser.Entity;
    }
}
