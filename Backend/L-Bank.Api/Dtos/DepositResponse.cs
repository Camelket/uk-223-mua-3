using System;

namespace L_Bank.Api.Dtos;

public class DepositResponse
{
    public decimal amount { get; set; }
    public DateTime date { get; set; }
    public int ledgerId { get; set; }
    public string? ledgerName { get; set; }
}
