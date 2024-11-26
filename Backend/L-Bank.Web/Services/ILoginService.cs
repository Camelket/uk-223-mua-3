using L_Bank_W_Backend.Core.Models;

namespace L_Bank_W_Backend.Services;

public interface ILoginService
{
    string CreateJwt(User? user);
}