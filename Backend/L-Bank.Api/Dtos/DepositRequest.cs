using System;

namespace L_Bank.Api.Dtos;

public class DepositRequest
{
    public int amount { get; set; }
    public int ledgerId { get; set; }
}
