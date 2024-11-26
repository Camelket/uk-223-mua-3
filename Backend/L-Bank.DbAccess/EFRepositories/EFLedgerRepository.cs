using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Interfaces;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFLedgerRepository(AppDbContext context) : IEFLedgerRepository
{
    private readonly AppDbContext context = context;
    public string Book(decimal amount, Ledger from, Ledger to)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Ledger> GetAllLedgers()
    {
        throw new NotImplementedException();
    }

    public decimal? GetBalance(int ledgerId)
    {
        throw new NotImplementedException();
    }

    public decimal? GetBalanceInTransaction(int ledgerId)
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

    public Ledger? SelectOneInTransaction(int id)
    {
        throw new NotImplementedException();
    }

    public void Update(Ledger ledger)
    {
        throw new NotImplementedException();
    }

    public void UpdateInTransaction(Ledger ledger)
    {
        throw new NotImplementedException();
    }
}
