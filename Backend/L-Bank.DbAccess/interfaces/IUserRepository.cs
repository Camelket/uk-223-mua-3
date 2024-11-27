using System.Data.SqlClient;
using L_Bank_W_Backend.Core.Models;

namespace L_Bank_W_Backend.Interfaces;

public interface IUserRepository
{
    User? Authenticate(string? username, string? password);
    User SelectOne(int id);
    void Update(User user, SqlConnection conn, SqlTransaction transaction);
    User Insert(User user, SqlConnection conn, SqlTransaction transaction);
    void Delete(int id, SqlConnection conn, SqlTransaction transaction);
}

public interface IEFUserRepository
{
    Task<User?> Authenticate(string username, string password);
    Task<User?> GetOne(int id, bool includeLedgers = false);
    Task<User> Save(User user);
}
