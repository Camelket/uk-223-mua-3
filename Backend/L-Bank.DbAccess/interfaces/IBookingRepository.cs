using L_Bank_W_Backend.Core.Models;

namespace L_Bank_W_Backend.Interfaces;

public interface IBookingRepository
{
    bool Book(int sourceLedgerId, int destinationLKedgerId, decimal amount);
}

public interface IEFBookingRepository
{
    Task<bool> Book(int sourceId, int targetId, decimal amount);
    Task<IEnumerable<Booking>> GetAllBookings();
    Task<Booking?> GetOne(int bookingId);
    Task<IEnumerable<Booking>> GetByLedger(int ledgerId);
    Task<IEnumerable<Booking>> GetbyUser(int userId);
}
