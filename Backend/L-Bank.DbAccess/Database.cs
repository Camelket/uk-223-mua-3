using System;

namespace L_Bank_W_Backend.DbAccess;

public interface IDatabase

public class Database
{
    public string ConnectionString;

    public Database(string ConnectionString)
    {
        this.ConnectionString = ConnectionString;
    } 
}
