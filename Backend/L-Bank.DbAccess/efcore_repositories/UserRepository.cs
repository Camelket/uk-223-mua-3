using System;
using System.Data.SqlClient;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.Services;

namespace L_Bank_W_Backend.DbAccess.efcore_repositories;

public class UserRepository : IUserRepository
{
    public User? Authenticate(string? username, string? password)
    {
        throw new NotImplementedException();
    }

    public void Delete(int id, SqlConnection conn, SqlTransaction transaction)
    {
        throw new NotImplementedException();
    }

    public User Insert(User user, SqlConnection conn, SqlTransaction transaction)
    {
        throw new NotImplementedException();
    }

    public User SelectOne(int id)
    {
        throw new NotImplementedException();
    }

    public void Update(User user, SqlConnection conn, SqlTransaction transaction)
    {
        throw new NotImplementedException();
    }
}
