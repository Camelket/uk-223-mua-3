using System.Runtime.CompilerServices;
using System.Security.Claims;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Interfaces;
using L_Bank_W_Backend.DbAccess.interfaces;
using L_Bank_W_Backend.Interfaces;
using L_Bank.Api.Dtos;
using L_Bank.Api.Helper;
using L_Bank.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace L_Bank.Api.Services;

public class BookingTransactionResult
{
    public ServiceStatus status;
    public string? message;
    public Booking? booking;
}

public class BankService(
    IEFBookingRepository bookingRepository,
    IEFUserRepository userRepository,
    IEFLedgerRepository ledgerRepository,
    IEFDepositRepository depositRepository,
    ILogger<BankService> logger
) : IBankService
{
    private readonly ILogger<BankService> logger = logger;
    private readonly IEFBookingRepository bookingRepository = bookingRepository;
    private readonly IEFUserRepository userRepository = userRepository;
    private readonly IEFLedgerRepository ledgerRepository = ledgerRepository;

    private readonly IEFDepositRepository depositRepository = depositRepository;

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
            var transactionResult = await strategy.ExecuteAsync(async () =>
            {
                using var transaction = bookingRepository.StartBookingTransaction();
                var result = await _Book(request.SourceId, request.TargetId, request.Amount);

                if (result.status != ServiceStatus.Success)
                {
                    await transaction.RollbackAsync();
                    return result;
                }

                await transaction.CommitAsync();
                return result;
            });

            if (
                transactionResult.status == ServiceStatus.Success
                && transactionResult.booking != null
            )
            {
                return DtoWrapper<BookingResponse>.WrapDto(
                    DtoMapper.ToBookingResponse(transactionResult.booking),
                    null
                );
            }
            return DtoWrapper<BookingResponse>.WrapDto(
                transactionResult.status,
                transactionResult.message ?? "Transaction was unable to complete"
            );
        }
        catch (Exception)
        {
            return DtoWrapper<BookingResponse>.WrapDto(
                ServiceStatus.TransactionFailed,
                "Transaction was unable to complete - Booking not recorded"
            );
        }
    }

    public async Task<BookingTransactionResult> _Book(int sourceId, int targetId, decimal amount)
    {
        var sourceLedger = await ledgerRepository.GetOne(sourceId);
        var targetLedger = await ledgerRepository.GetOne(targetId);

        // Thread.Sleep(5000);

        if (sourceLedger == null || targetLedger == null)
        {
            return new BookingTransactionResult()
            {
                status = ServiceStatus.NotFound,
                message = "Source or Target Ledger doesnt exist",
                booking = null,
            };
        }

        if (!(amount > 0))
        {
            return new BookingTransactionResult()
            {
                status = ServiceStatus.BadRequest,
                message = "Cannot transfer amounts which arent greater than 0",
                booking = null,
            };
        }

        if (sourceLedger.Balance < amount)
        {
            return new BookingTransactionResult()
            {
                status = ServiceStatus.BadRequest,
                message = "Not enough money to transfer target amount",
                booking = null,
            };
        }

        sourceLedger.Balance -= amount;
        targetLedger.Balance += amount;

        var booking = new Booking
        {
            Amount = amount,
            Source = sourceLedger,
            Destination = targetLedger,
            Date = DateTime.Now,
        };

        await ledgerRepository.Save(sourceLedger);
        await ledgerRepository.Save(targetLedger);
        await bookingRepository.Save(booking);

        return new BookingTransactionResult()
        {
            status = ServiceStatus.Success,
            message = null,
            booking = booking,
        };
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

    public async Task<DtoWrapper<List<SimpleLedgerResponse>>> GetAllLedgersInSimpleForm()
    {
        try
        {
            var ledgers = await ledgerRepository.GetAllLedgers();
            return DtoWrapper<List<SimpleLedgerResponse>>.WrapDto(
                ledgers.Select(x => DtoMapper.ToSimpleLedgerResponse(x)).ToList(),
                null
            );
        }
        catch (Exception ex)
        {
            return DtoWrapper<List<SimpleLedgerResponse>>.WrapDto(
                ServiceStatus.Failed,
                $"{ex.Message}"
            );
        }
    }

    public async Task<DtoWrapper<List<DepositResponse>>> GetAllDeposits()
    {
        var deposits = await depositRepository.GetAllDeposits();
        return DtoWrapper<List<DepositResponse>>.WrapDto(
            deposits.Select(x => DtoMapper.ToDepositResponse(x)).ToList(),
            null
        );
    }

    public async Task<DtoWrapper<List<DepositResponse>>> GetDepositsForLedger(int ledgerId)
    {
        var deposits = await depositRepository.GetDepositsByLedger(ledgerId);
        return DtoWrapper<List<DepositResponse>>.WrapDto(
            deposits.Select(x => DtoMapper.ToDepositResponse(x)).ToList(),
            null
        );
    }

    public async Task<DtoWrapper<DepositResponse>> MakeDeposit(DepositRequest request, int userId)
    {
        var userIsOwner = await LedgerBelongsToUser(request.ledgerId, userId);
        if (userIsOwner)
        {
            if (request.amount < 0)
            {
                return await _MakeWithdrawl(request, userId);
            }
            else
            {
                return await _MakeDeposit(request, userId);
            }
        }
        return DtoWrapper<DepositResponse>.WrapDto(
            ServiceStatus.BadRequest,
            "Not your Ledger scrub"
        );
    }

    private async Task<DtoWrapper<DepositResponse>> _MakeDeposit(DepositRequest request, int userId)
    {
        var strategy = bookingRepository.StartRetryExecution(5);
        try
        {
            var transactionResult = await strategy.ExecuteAsync(async () =>
            {
                using var transaction = bookingRepository.StartBookingTransaction();
                var ledger = await ledgerRepository.GetOne(request.ledgerId);
                if (ledger == null)
                {
                    return new DtoWrapper<DepositResponse>()
                    {
                        Status = ServiceStatus.BadRequest,
                        Message = "Ledger doesnt exist",
                    };
                }
                else
                {
                    ledger.Balance += request.amount;
                    var resultLedger = await ledgerRepository.Save(ledger);
                    var deposit = await depositRepository.Save(
                        new Deposit()
                        {
                            LedgerId = resultLedger.Id,
                            DepositorId = userId,
                            Amount = request.amount,
                            date = DateTime.Now,
                        }
                    );
                    await transaction.CommitAsync();
                    return new DtoWrapper<DepositResponse>()
                    {
                        Status = ServiceStatus.Success,
                        Data = DtoMapper.ToDepositResponse(deposit),
                        Message = "Success",
                    };
                }
            });
            return transactionResult;
        }
        catch (Exception)
        {
            return DtoWrapper<DepositResponse>.WrapDto(
                ServiceStatus.TransactionFailed,
                "Transaction was unable to complete - Withdrawl not recorded"
            );
        }
    }

    private async Task<DtoWrapper<DepositResponse>> _MakeWithdrawl(
        DepositRequest request,
        int userId
    )
    {
        var strategy = bookingRepository.StartRetryExecution(5);
        try
        {
            var transactionResult = await strategy.ExecuteAsync(async () =>
            {
                using var transaction = bookingRepository.StartBookingTransaction();
                var ledger = await ledgerRepository.GetOne(request.ledgerId);
                if (ledger == null)
                {
                    return new DtoWrapper<DepositResponse>()
                    {
                        Status = ServiceStatus.BadRequest,
                        Message = "Ledger doesnt exist",
                    };
                }
                if (ledger.Balance < request.amount)
                {
                    await transaction.RollbackAsync();
                    return new DtoWrapper<DepositResponse>()
                    {
                        Status = ServiceStatus.BadRequest,
                        Message = "Insufficient balance for withdrawal",
                    };
                }
                else
                {
                    ledger.Balance += request.amount;
                    var resultLedger = await ledgerRepository.Save(ledger);
                    var deposit = await depositRepository.Save(
                        new Deposit()
                        {
                            LedgerId = resultLedger.Id,
                            DepositorId = userId,
                            Amount = request.amount,
                            date = DateTime.Now,
                        }
                    );
                    await transaction.CommitAsync();
                    return new DtoWrapper<DepositResponse>()
                    {
                        Status = ServiceStatus.Success,
                        Data = DtoMapper.ToDepositResponse(deposit),
                        Message = "Success",
                    };
                }
            });
            return transactionResult;
        }
        catch (Exception)
        {
            return DtoWrapper<DepositResponse>.WrapDto(
                ServiceStatus.TransactionFailed,
                "Transaction was unable to complete - Withdrawl not recorded"
            );
        }
    }

    public async Task<DtoWrapper<List<DepositResponse>>> GetDepositsByUser(int userId)
    {
        var result = await depositRepository.GetDepositsByUser(userId);
        return DtoWrapper<List<DepositResponse>>.WrapDto(
            result.Select(DtoMapper.ToDepositResponse).ToList(),
            null
        );
    }

    public async Task<DtoWrapper<decimal>> GetTotalMoney()
    {
        try
        {
            var result = await ledgerRepository.GetTotalMoney();
            return DtoWrapper<decimal>.WrapDto(result, null);
        }
        catch (Exception)
        {
            return DtoWrapper<decimal>.WrapDto(
                ServiceStatus.Failed,
                "Cannot get total money in bank"
            );
        }
    }
}
