using System;

namespace L_Bank.Api.Dtos;

public class LedgerResponse
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Balance { get; set; }
}
