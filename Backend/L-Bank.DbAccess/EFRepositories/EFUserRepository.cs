using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.Interfaces;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFUserRepository(AppDbContext context) : IEFUserRepository
{
    private readonly AppDbContext context = context;

    public Task<User?> Authenticate(string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetOne(int id)
    {
        throw new NotImplementedException();
    }

    public Task<User> Save(User user)
    {
        throw new NotImplementedException();
    }
}
