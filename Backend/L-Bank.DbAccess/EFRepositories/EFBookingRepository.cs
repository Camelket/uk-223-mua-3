using System.Data;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFBookingRepository(AppDbContext context, ILogger<EFBookingRepository> logger)
    : IEFBookingRepository
{
    private readonly AppDbContext context = context;
    private readonly ILogger _logger = logger;

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

    public async Task<Booking?> GetOneWithLedgers(int bookingId)
    {
        return await context
            .Set<Booking>()
            .AsNoTracking()
            .Include(x => x.Source)
            .Include(x => x.Destination)
            .FirstAsync(x => x.Id == bookingId);
    }

    public SqlServerRetryingExecutionStrategy StartRetryExecution(int maxRetry)
    {
        return new SqlServerRetryingExecutionStrategy(context, maxRetry);
    }

    public IDbContextTransaction StartBookingTransaction()
    {
        return context.Database.BeginTransaction(IsolationLevel.Serializable);
    }

    public async Task<Booking> Save(Booking booking)
    {
        context.Add(booking);
        await context.SaveChangesAsync();
        return booking;
    }
}
