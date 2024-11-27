using L_Bank_W_Backend.Core.Models;
using L_Bank.Api.Dtos;

namespace L_Bank.Api.Services;

public interface IBankService
{
    // including ledgers
    Task<UserResponse> GetUser(int userId);

    Task<LedgerResponse> GetLedger(int ledgerId);
    Task<List<LedgerResponse>> GetAllLedgers();

    Task<BookingResponse> GetBooking(int bookingId);
    Task<List<Booking>> GetAllBookings();
    Task<List<Booking>> GetBookingsForLedger(int ledgerId);
    Task<List<Booking>> GetBookingsForUser(int userId);

    Task<Booking> NewBooking(BookingRequest request);
}
