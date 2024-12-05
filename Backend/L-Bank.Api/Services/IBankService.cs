using L_Bank.Api.Dtos;

namespace L_Bank.Api.Services;

public interface IBankService
{
    // including ledgers
    Task<DtoWrapper<UserResponse>> GetUserWithLedgers(int userId);

    Task<DtoWrapper<decimal>> GetTotalMoney();

    Task<DtoWrapper<LedgerResponse>> GetLedger(int ledgerId);
    Task<DtoWrapper<List<LedgerResponse>>> GetAllLedgers();
    Task<DtoWrapper<List<SimpleLedgerResponse>>> GetAllLedgersInSimpleForm();

    Task<DtoWrapper<List<BookingResponse>>> GetAllBookings();
    Task<DtoWrapper<BookingResponse>> GetBooking(int bookingId);
    Task<DtoWrapper<List<BookingResponse>>> GetBookingsForLedger(int ledgerId);
    Task<DtoWrapper<List<BookingResponse>>> GetBookingsForUser(int userId);

    Task<DtoWrapper<LedgerResponse>> NewLedger(LedgerRequest request, int userId);

    Task<bool> LedgerBelongsToUser(int ledgerId, int userId);
    Task<bool> BookingBelongsToUser(int bookingId, int userId);

    Task<DtoWrapper<List<DepositResponse>>> GetAllDeposits();
    Task<DtoWrapper<List<DepositResponse>>> GetDepositsForLedger(int ledgerId);
    Task<DtoWrapper<List<DepositResponse>>> GetDepositsByUser(int userId);
    Task<DtoWrapper<DepositResponse>> MakeDeposit(DepositRequest request, int userId);
    Task<DtoWrapper<BookingResponse>> NewBookingWithProcedure(BookingRequest request);

    Task<DtoWrapper<BookingResponse>> NewBookingWithProcedureAndQueue(BookingRequest request);

    Task<DtoWrapper<BookingResponse>> NewBookingWithQueue(BookingRequest request);
}
