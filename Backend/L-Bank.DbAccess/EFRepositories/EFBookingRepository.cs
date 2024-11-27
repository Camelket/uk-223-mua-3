using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Interfaces;
using L_Bank_W_Backend.Interfaces;
using Microsoft.Extensions.Logging;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFBookingRepository(
    AppDbContext context,
    IEFLedgerRepository ledgerRepository,
    ILogger logger
) : IEFBookingRepository
{
    private readonly AppDbContext context = context;
    private readonly ILogger _logger = logger;
    private readonly IEFLedgerRepository ledgerRepository = ledgerRepository;

    async Task<bool> IEFBookingRepository.Book(int sourceId, int targetId, decimal amount)
    {
        context.Database.BeginTransaction();
        var sourceLedger = await ledgerRepository.GetOne(sourceId);
        var targetLedger = await ledgerRepository.GetOne(targetId);
        if (sourceLedger == null || targetLedger == null)
        {
            context.Database.RollbackTransaction();
            return false;
        }

        if (sourceLedger.Balance < amount)
        {
            context.Database.RollbackTransaction();
            _logger.LogDebug("Not enough money in source ledger");
            return false;
        }

        sourceLedger.Balance -= amount;
        targetLedger.Balance += amount;

        context
            .Set<Booking>()
            .Add(
                new Booking
                {
                    Amount = amount,
                    SourceId = sourceLedger.Id,
                    DestinationId = targetLedger.Id,
                    Date = DateTime.Now,
                }
            );
        await ledgerRepository.Save(sourceLedger);
        await ledgerRepository.Save(targetLedger);
        await context.SaveChangesAsync();
        context.Database.CommitTransaction();
        return true;
    }
}
