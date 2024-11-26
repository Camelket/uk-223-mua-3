using L_Bank_W_Backend.Core.Models;

namespace L_Bank.Core.Helper;

public static class PasswordHelper
{
    public static string HashAndSaltPassword(string clearTextPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(clearTextPassword);
    }
    
    public static bool VerifyPassword(string? clearTextPassword, User user)
    {
        return BCrypt.Net.BCrypt.Verify(clearTextPassword, user.PasswordHash);
    }
}