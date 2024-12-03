using System;

namespace L_Bank.Api.Dtos;

public class DepositResponse
{
    public int DepositId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int LedgerId { get; set; }
    public string? LedgerName { get; set; }
}
