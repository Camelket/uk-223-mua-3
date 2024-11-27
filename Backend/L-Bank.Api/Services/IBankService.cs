using L_Bank.Api.Dtos;

namespace L_Bank.Api.Services;

public interface IBankService
{
    // including ledgers
    Task<DtoWrapper<UserResponse>> GetUserWithLedgers(int userId);

    Task<DtoWrapper<LedgerResponse>> GetLedger(int ledgerId);
    Task<DtoWrapper<List<LedgerResponse>>> GetAllLedgers();

    Task<DtoWrapper<List<BookingResponse>>> GetAllBookings();
    Task<DtoWrapper<BookingResponse>> GetBooking(int bookingId);
    Task<DtoWrapper<List<BookingResponse>>> GetBookingsForLedger(int ledgerId);
    Task<DtoWrapper<List<BookingResponse>>> GetBookingsForUser(int userId);

    Task<DtoWrapper<BookingResponse>> NewBooking(BookingRequest request);
}
