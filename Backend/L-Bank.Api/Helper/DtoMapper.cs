using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank.Api.Dtos;
using L_Bank.Core.Models;

namespace L_Bank.Api.Helper;

public static class DtoMapper
{
    public static DepositResponse ToDepositResponse(Deposit deposit)
    {
        return new DepositResponse()
        {
            DepositId = deposit.Id,
            Amount = deposit.Amount,
            Date = deposit.date,
            LedgerId = deposit.LedgerId,
            LedgerName = deposit.Ledger?.Name,
        };
    }

    public static LedgerResponse ToLedgerResponse(Ledger ledger)
    {
        return new LedgerResponse()
        {
            Id = ledger.Id,
            UserId = ledger.UserId,
            Name = ledger.Name,
            Balance = ledger.Balance,
        };
    }

    public static SimpleLedgerResponse ToSimpleLedgerResponse(Ledger ledger)
    {
        return new SimpleLedgerResponse() { Id = ledger.Id, Name = ledger.Name };
    }

    public static UserResponse ToUserResponse(User user)
    {
        return new UserResponse()
        {
            Id = user.Id,
            Username = user.Username,
            Role = nameof(user.Role),
            Ledgers = user.Ledgers.Select(l => ToLedgerResponse(l)).ToList(),
        };
    }

    public static BookingResponse ToBookingResponse(Booking booking)
    {
        return new BookingResponse()
        {
            Id = booking.Id,
            SourceId = booking.SourceId,
            SourceName = booking.Source?.Name,
            TargetId = booking.DestinationId,
            TargetName = booking.Destination?.Name,
            TransferedAmount = booking.Amount,
            Date = booking.Date,
        };
    }
}
