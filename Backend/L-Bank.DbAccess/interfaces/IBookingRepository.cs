using L_Bank_W_Backend.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace L_Bank_W_Backend.Interfaces;

public interface IBookingRepository
{
    bool Book(int sourceLedgerId, int destinationLKedgerId, decimal amount);
}

public interface IEFBookingRepository
{
    Task<IEnumerable<Booking>> GetAllBookings();
    Task<Booking?> GetOne(int bookingId);
    Task<Booking?> GetOneWithLedgers(int bookingId);
    Task<IEnumerable<Booking>> GetByLedger(int ledgerId);
    Task<IEnumerable<Booking>> GetbyUser(int userId);
    IExecutionStrategy StartRetryExecution(int maxRetry, TimeSpan retryDelay);
    IDbContextTransaction StartBookingTransaction();
    Task<Booking> Save(Booking booking);
    void LockBookingTable();
}
