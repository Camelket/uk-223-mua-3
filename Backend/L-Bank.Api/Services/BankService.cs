using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Interfaces;
using L_Bank_W_Backend.Interfaces;
using L_Bank.Api.Dtos;

namespace L_Bank.Api.Services;

public class BankService(
    IEFBookingRepository bookingRepository,
    IEFUserRepository userRepository,
    IEFLedgerRepository ledgerRepository
) : IBankService
{
    private readonly IEFBookingRepository bookingRepository = bookingRepository;
    private readonly IEFUserRepository userRepository = userRepository;
    private readonly IEFLedgerRepository ledgerRepository = ledgerRepository;

    public Task<DtoWrapper<List<Booking>>> GetAllBookings()
    {
        throw new NotImplementedException();
    }

    public async Task<DtoWrapper<List<LedgerResponse>>> GetAllLedgers()
    {
        var ledgers = await ledgerRepository.GetAllLedgers();
        
        throw new NotImplementedException();
    }

    public Task<DtoWrapper<BookingResponse>> GetBooking(int bookingId)
    {
        throw new NotImplementedException();
    }

    public Task<DtoWrapper<List<Booking>>> GetBookingsForLedger(int ledgerId)
    {
        throw new NotImplementedException();
    }

    public Task<DtoWrapper<List<Booking>>> GetBookingsForUser(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<DtoWrapper<LedgerResponse>> GetLedger(int ledgerId)
    {
        throw new NotImplementedException();
    }

    public Task<DtoWrapper<UserResponse>> GetUser(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<DtoWrapper<Booking>> NewBooking(BookingRequest request)
    {
        throw new NotImplementedException();
    }
}
