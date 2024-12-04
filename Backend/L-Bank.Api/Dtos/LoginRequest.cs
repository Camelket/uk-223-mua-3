using System;

namespace L_Bank.Api.Dtos;

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
