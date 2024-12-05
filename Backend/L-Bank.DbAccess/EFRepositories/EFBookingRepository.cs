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
        return await context
            .Set<Booking>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == bookingId);
    }

    public async Task<Booking?> GetOneWithLedgers(int bookingId)
    {
        return await context
            .Set<Booking>()
            .AsNoTracking()
            .Include(x => x.Source)
            .Include(x => x.Destination)
            .FirstOrDefaultAsync(x => x.Id == bookingId);
    }

    public void LockBookingTable()
    {
        context.Database.ExecuteSql($"SELECT TOP 1 Id FROM Bookings WITH (TABLOCKX, HOLDLOCK)");
    }

    public IExecutionStrategy StartRetryExecution(int maxRetry, TimeSpan retryDelay)
    {
        return context.Database.CreateExecutionStrategy();
        // return new SqlServerRetryingExecutionStrategy(context, maxRetry, retryDelay, null);
    }

    public IDbContextTransaction StartBookingTransaction()
    {
        return context.Database.BeginTransaction();
    }

    public async Task<Booking> Save(Booking booking)
    {
        if (booking.Id == 0)
        {
            context.Update(booking);
        }
        else
        {
            context.Add(booking);
        }

        await context.SaveChangesAsync();
        return booking;
    }

    public async Task<bool> BookPrc(decimal amount, int from, int to)
    {
        var strategy = new SqlServerRetryingExecutionStrategy(context);
        var result = await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await context.Database.ExecuteSqlAsync(
                    $@"EXEC TransferAmount @sourceId = {from}, @targetId = {to}, @amount = {amount};"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        });
        return result;
    }
}
