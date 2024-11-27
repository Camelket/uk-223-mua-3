using System;

namespace L_Bank.Api.Dtos;

public class BookingRequest
{
    public int SourceId { get; set; }
    public int TargetId { get; set; }
    public decimal Amount { get; set; }
}
