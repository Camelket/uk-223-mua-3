using L_Bank_W_Backend.Core.Models;

namespace L_Bank.Api.Services;

public interface IAuthService
{
    public string CreateJwt(User? user);
}
