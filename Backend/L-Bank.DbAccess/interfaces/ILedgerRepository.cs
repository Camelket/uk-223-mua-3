using System.Data.SqlClient;
using L_Bank_W_Backend.Core.Models;

namespace L_Bank_W_Backend.DbAccess.Interfaces;

public interface ILedgerRepository
{
    IEnumerable<Ledger> GetAllLedgers();
    public string Book(decimal amount, Ledger from, Ledger to);
    decimal GetTotalMoney();
    Ledger? SelectOne(int id);
    Ledger? SelectOne(int id, SqlConnection conn, SqlTransaction? transaction);
    void Update(Ledger ledger, SqlConnection conn, SqlTransaction transaction);
    void Update(Ledger ledger);
    decimal? GetBalance(int ledgerId, SqlConnection conn, SqlTransaction transaction);
}

public interface IEFLedgerRepository
{
    Task<IEnumerable<Ledger>> GetAllLedgers();
    Task<decimal> GetTotalMoney();
    Task<Ledger?> GetOne(int id);
    Task<Ledger> Save(Ledger ledger);
    void LockLedgersTable();
}
