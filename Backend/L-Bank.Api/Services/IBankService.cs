using L_Bank_W_Backend.Core.Models;
using L_Bank.Api.Dtos;

namespace L_Bank.Api.Services;

public interface IBankService
{
    // including ledgers
    Task<DtoWrapper<UserResponse>> GetUser(int userId);

    Task<DtoWrapper<LedgerResponse>> GetLedger(int ledgerId);
    Task<DtoWrapper<List<LedgerResponse>>> GetAllLedgers();

    Task<DtoWrapper<BookingResponse>> GetBooking(int bookingId);
    Task<DtoWrapper<List<Booking>>> GetAllBookings();
    Task<DtoWrapper<List<Booking>>> GetBookingsForLedger(int ledgerId);
    Task<DtoWrapper<List<Booking>>> GetBookingsForUser(int userId);

    Task<DtoWrapper<Booking>> NewBooking(BookingRequest request);
}
