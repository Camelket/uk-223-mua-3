using System;

namespace L_Bank.Api.Dtos;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
