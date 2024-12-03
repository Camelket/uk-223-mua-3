using System;

namespace L_Bank.Api.Dtos;

public class DepositRequest
{
    public decimal Amount { get; set; }
    public int LedgerId { get; set; }
}
