using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Interfaces;
using L_Bank_W_Backend.Interfaces;
using L_Bank.Api.Dtos;
using L_Bank.Api.Helper;
using Microsoft.EntityFrameworkCore;

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

    public async Task<DtoWrapper<LedgerResponse>> NewLedger(LedgerRequest request, int userId)
    {
        try
        {
            var ledger = await ledgerRepository.Save(
                new Ledger { Name = request.Name, UserId = userId }
            );
            return DtoWrapper<LedgerResponse>.WrapDto(DtoMapper.ToLedgerResponse(ledger), null);
        }
        catch (Exception ex)
        {
            return DtoWrapper<LedgerResponse>.WrapDto(ServiceStatus.Failed, $"{ex.Message}");
        }
    }

    public async Task<DtoWrapper<List<BookingResponse>>> GetAllBookings()
    {
        try
        {
            var bookings = await bookingRepository.GetAllBookings();
            return DtoWrapper<List<BookingResponse>>.WrapDto(
                bookings.Select(x => DtoMapper.ToBookingResponse(x)).ToList(),
                null
            );
        }
        catch (Exception ex)
        {
            return DtoWrapper<List<BookingResponse>>.WrapDto(ServiceStatus.Failed, $"{ex.Message}");
        }
    }

    public async Task<DtoWrapper<List<LedgerResponse>>> GetAllLedgers()
    {
        try
        {
            var ledgers = await ledgerRepository.GetAllLedgers();
            return DtoWrapper<List<LedgerResponse>>.WrapDto(
                ledgers.Select(x => DtoMapper.ToLedgerResponse(x)).ToList(),
                null
            );
        }
        catch (Exception ex)
        {
            return DtoWrapper<List<LedgerResponse>>.WrapDto(ServiceStatus.Failed, $"{ex.Message}");
        }
    }

    public async Task<DtoWrapper<BookingResponse>> GetBooking(int bookingId)
    {
        try
        {
            var booking = await bookingRepository.GetOne(bookingId);
            if (booking != null)
            {
                return DtoWrapper<BookingResponse>.WrapDto(
                    DtoMapper.ToBookingResponse(booking),
                    null
                );
            }
            return DtoWrapper<BookingResponse>.WrapDto(
                ServiceStatus.NotFound,
                "Booking doesnt exist"
            );
        }
        catch (Exception ex)
        {
            return DtoWrapper<BookingResponse>.WrapDto(ServiceStatus.Failed, $"{ex.Message}");
        }
    }

    public async Task<DtoWrapper<List<BookingResponse>>> GetBookingsForLedger(int ledgerId)
    {
        try
        {
            var bookings = await bookingRepository.GetByLedger(ledgerId);
            return DtoWrapper<List<BookingResponse>>.WrapDto(
                bookings.Select(x => DtoMapper.ToBookingResponse(x)).ToList(),
                null
            );
        }
        catch (Exception ex)
        {
            return DtoWrapper<List<BookingResponse>>.WrapDto(ServiceStatus.Failed, $"{ex.Message}");
        }
    }

    public async Task<DtoWrapper<List<BookingResponse>>> GetBookingsForUser(int userId)
    {
        try
        {
            var bookings = await bookingRepository.GetbyUser(userId);
            return DtoWrapper<List<BookingResponse>>.WrapDto(
                bookings.Select(x => DtoMapper.ToBookingResponse(x)).ToList(),
                null
            );
        }
        catch (Exception ex)
        {
            return DtoWrapper<List<BookingResponse>>.WrapDto(ServiceStatus.Failed, $"{ex.Message}");
        }
    }

    public async Task<DtoWrapper<LedgerResponse>> GetLedger(int ledgerId)
    {
        try
        {
            var ledger = await ledgerRepository.GetOne(ledgerId);
            if (ledger != null)
            {
                return DtoWrapper<LedgerResponse>.WrapDto(DtoMapper.ToLedgerResponse(ledger), null);
            }
            return DtoWrapper<LedgerResponse>.WrapDto(
                ServiceStatus.NotFound,
                "Ledger doesnt exist"
            );
        }
        catch (Exception ex)
        {
            return DtoWrapper<LedgerResponse>.WrapDto(ServiceStatus.Failed, $"{ex.Message}");
        }
    }

    public Task<DtoWrapper<UserResponse>> GetUser(int userId)
    {
        throw new NotImplementedException();
    }

    public async Task<DtoWrapper<UserResponse>> GetUserWithLedgers(int userId)
    {
        try
        {
            var user = await userRepository.GetOne(userId, true);
            if (user != null)
            {
                return DtoWrapper<UserResponse>.WrapDto(DtoMapper.ToUserResponse(user), null);
            }
            return DtoWrapper<UserResponse>.WrapDto(ServiceStatus.NotFound, "User doesnt exist");
        }
        catch (Exception ex)
        {
            return DtoWrapper<UserResponse>.WrapDto(ServiceStatus.Failed, $"{ex.Message}");
        }
    }

    public async Task<DtoWrapper<BookingResponse>> NewBooking(BookingRequest request)
    {
        var strategy = bookingRepository.StartRetryExecution(5);
        try
        {
            var booking = await strategy.ExecuteAsync(async () =>
            {
                using var transaction = bookingRepository.StartBookingTransaction();
                var result = await _Book(request.SourceId, request.TargetId, request.Amount);

                if (result == null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                await transaction.CommitAsync();
                return result;
            });

            if (booking != null)
            {
                return DtoWrapper<BookingResponse>.WrapDto(
                    DtoMapper.ToBookingResponse(booking),
                    null
                );
            }
            return DtoWrapper<BookingResponse>.WrapDto(
                ServiceStatus.TransactionFailed,
                "Transaction was unable to complete - Booking not recorded"
            );
        }
        catch (Exception ex)
        {
            return DtoWrapper<BookingResponse>.WrapDto(ServiceStatus.Failed, $"{ex.Message}");
        }
    }

    public async Task<Booking?> _Book(int sourceId, int targetId, decimal amount)
    {
        var sourceLedger = await ledgerRepository.GetOne(sourceId);
        var targetLedger = await ledgerRepository.GetOne(targetId);

        if (sourceLedger == null || targetLedger == null)
        {
            return null;
        }

        if (sourceLedger.Balance < amount)
        {
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

        await ledgerRepository.Save(sourceLedger);
        await ledgerRepository.Save(targetLedger);
        await bookingRepository.Save(booking);

        return booking;
    }

    public async Task<bool> LedgerBelongsToUser(int ledgerId, int userId)
    {
        var ledger = await ledgerRepository.GetOne(ledgerId);
        return ledger?.UserId == userId;
    }

    public async Task<bool> BookingBelongsToUser(int bookingId, int userId)
    {
        var booking = await bookingRepository.GetOneWithLedgers(bookingId);
        if (booking != null && booking.Source != null && booking.Destination != null)
        {
            return userId == booking.Source.UserId || userId == booking.Destination.UserId;
        }
        return false;
    }
}
