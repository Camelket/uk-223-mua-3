using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.Interfaces;
using L_Bank.Core.Helper;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFUserRepository(AppDbContext context) : IEFUserRepository
{
    private readonly AppDbContext context = context;

    public async Task<User?> Authenticate(string username, string password)
    {
        var user = await context.Set<User>().AsNoTracking().FirstAsync(x => x.Username == username);
        var validPassword = PasswordHelper.VerifyPassword(password, user);
        if (validPassword)
        {
            return user;
        }
        return null;
    }

    public async Task<User?> GetOne(int id)
    {
        return await context.Set<User>().AsNoTracking().FirstAsync(x => x.Id == id);
    }

    public async Task<User> Save(User user)
    {
        var SavedUser = await context.AddAsync(user);
        await context.SaveChangesAsync();
        return SavedUser.Entity;
    }
}
