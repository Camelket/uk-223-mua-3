using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank.Api.Dtos;

namespace L_Bank.Api.Helper;

public static class DtoMapper
{
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
            TargetId = booking.DestinationId,
            TransferedAmount = booking.Amount,
            Date = booking.Date,
        };
    }
}
