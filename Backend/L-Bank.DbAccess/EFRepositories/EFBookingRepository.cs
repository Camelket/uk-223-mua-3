using System.Data;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Interfaces;
using L_Bank_W_Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFBookingRepository(
    AppDbContext context,
    IEFLedgerRepository ledgerRepository,
    ILogger<EFBookingRepository> logger
) : IEFBookingRepository
{
    private readonly AppDbContext context = context;
    private readonly ILogger _logger = logger;
    private readonly IEFLedgerRepository ledgerRepository = ledgerRepository;

    public async Task<IEnumerable<Booking>> GetAllBookings()
    {
        return await context.Set<Booking>().AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetByLedger(int ledgerId)
    {
        return await context
            .Set<Booking>()
            .Where(x => x.SourceId == ledgerId || x.DestinationId == ledgerId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetbyUser(int userId)
    {
        return await context
            .Set<Booking>()
            .Include(x => x.Source)
            .Include(x => x.Destination)
            .Where(x => x.Source.UserId == userId || x.Destination.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Booking?> GetOne(int bookingId)
    {
        return await context.Set<Booking>().AsNoTracking().FirstAsync(x => x.Id == bookingId);
    }

    async Task<Booking?> IEFBookingRepository.Book(int sourceId, int targetId, decimal amount)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.Execute(async () =>
        {
            using var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable);

            var sourceLedger = await ledgerRepository.GetOne(sourceId);
            var targetLedger = await ledgerRepository.GetOne(targetId);
            if (sourceLedger == null || targetLedger == null)
            {
                context.Database.RollbackTransaction();
                return null;
            }

            if (sourceLedger.Balance < amount)
            {
                context.Database.RollbackTransaction();
                _logger.LogDebug("Not enough money in source ledger");
                return null;
            }

            sourceLedger.Balance -= amount;
            targetLedger.Balance += amount;

            var booking = new Booking
            {
                Amount = amount,
                SourceId = sourceLedger.Id,
                DestinationId = targetLedger.Id,
                Date = DateTime.Now,
            };

            context.Set<Booking>().Add(booking);
            await ledgerRepository.Save(sourceLedger);
            await ledgerRepository.Save(targetLedger);

            await context.SaveChangesAsync();

            await transaction.CommitAsync();
            return booking;
        });
    }
}
