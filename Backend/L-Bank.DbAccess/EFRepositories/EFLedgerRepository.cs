using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFLedgerRepository(AppDbContext context) : IEFLedgerRepository
{
    private readonly AppDbContext context = context;

    public async Task<IEnumerable<Ledger>> GetAllLedgers()
    {
        return await context.Set<Ledger>().AsNoTracking().ToListAsync();
    }

    public async Task<Ledger?> GetOne(int id)
    {
        return await context.Set<Ledger>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<decimal> GetTotalMoney()
    {
        return await context.Set<Ledger>().SumAsync(x => x.Balance);
    }

    public async Task<Ledger> Save(Ledger ledger)
    {
        if (ledger.Id != 0)
        {
            context.Update(ledger);
        }
        else
        {
            context.Add(ledger);
        }

        await context.SaveChangesAsync();
        return ledger;
    }
}
