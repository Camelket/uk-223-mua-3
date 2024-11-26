using System;
using System.Data.SqlClient;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Repositories;

namespace L_Bank_W_Backend.DbAccess.efcore_repositories;

public class LedgerRepository : ILedgerRepository
{
    public string Book(decimal amount, Ledger from, Ledger to)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Ledger> GetAllLedgers()
    {
        throw new NotImplementedException();
    }

    public decimal? GetBalance(int ledgerId, SqlConnection conn, SqlTransaction transaction)
    {
        throw new NotImplementedException();
    }

    public decimal GetTotalMoney()
    {
        throw new NotImplementedException();
    }

    public Ledger? SelectOne(int id)
    {
        throw new NotImplementedException();
    }

    public Ledger? SelectOne(int id, SqlConnection conn, SqlTransaction? transaction)
    {
        throw new NotImplementedException();
    }

    public void Update(Ledger ledger, SqlConnection conn, SqlTransaction transaction)
    {
        throw new NotImplementedException();
    }

    public void Update(Ledger ledger)
    {
        throw new NotImplementedException();
    }
}
