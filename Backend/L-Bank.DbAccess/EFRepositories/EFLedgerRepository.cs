using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Interfaces;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFLedgerRepository(AppDbContext context) : IEFLedgerRepository
{
    private readonly AppDbContext context = context;

    public Task<IEnumerable<Ledger>> GetAllLedgers()
    {
        throw new NotImplementedException();
    }

    public Task<Ledger?> GetOne(int id)
    {
        throw new NotImplementedException();
    }

    public Task<decimal> GetTotalMoney()
    {
        throw new NotImplementedException();
    }

    public Task<Ledger> Save(Ledger ledger)
    {
        throw new NotImplementedException();
    }
}
