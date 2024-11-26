using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.Interfaces;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFUserRepository(AppDbContext context) : IEFUserRepository
{
    private readonly AppDbContext context = context;
    public User? Authenticate(string? username, string? password)
    {
        throw new NotImplementedException();
    }

    public void Delete(int id)
    {
        throw new NotImplementedException();
    }

    public void DeleteInTransaction(int id)
    {
        throw new NotImplementedException();
    }

    public User Insert(User user)
    {
        throw new NotImplementedException();
    }

    public User InsertInTransaction(User user)
    {
        throw new NotImplementedException();
    }

    public User SelectOne(int id)
    {
        throw new NotImplementedException();
    }

    public void Update(User user)
    {
        throw new NotImplementedException();
    }

    public void UpdateInTransaction(User user)
    {
        throw new NotImplementedException();
    }
}
