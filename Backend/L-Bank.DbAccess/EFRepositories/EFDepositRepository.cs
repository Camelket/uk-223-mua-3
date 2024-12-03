using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.interfaces;
using L_Bank.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFDepositRepository(AppDbContext context) : IEFDepositRepository
{
    private readonly AppDbContext context = context;

    public async Task<IEnumerable<Deposit>> GetAllDeposits()
    {
        return await context.Set<Deposit>().AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Deposit>> GetDepositsByUser(int userId)
    {
        return await context
            .Set<Deposit>()
            .AsNoTracking()
            .Include(x => x.Ledger)
            .Where(x => x.DepositorId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Deposit>> GetDepositsByLedger(int ledgerId)
    {
        return await context
            .Set<Deposit>()
            .AsNoTracking()
            .Where(x => x.LedgerId == ledgerId)
            .ToListAsync();
    }

    public async Task<Deposit> Save(Deposit deposit)
    {
        if (deposit.Id != 0)
        {
            context.Update(deposit);
        }
        else
        {
            context.Add(deposit);
        }

        await context.SaveChangesAsync();
        return deposit;
    }
}
