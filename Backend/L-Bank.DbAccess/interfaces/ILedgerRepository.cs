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
    IEnumerable<Ledger> GetAllLedgers();
    string Book(decimal amount, Ledger from, Ledger to);
    decimal GetTotalMoney();
    Ledger? SelectOne(int id);
    void Update(Ledger ledger);
    decimal? GetBalance(int ledgerId);
    Ledger? SelectOneInTransaction(int id);
    void UpdateInTransaction(Ledger ledger);
    decimal? GetBalanceInTransaction(int ledgerId);
}
