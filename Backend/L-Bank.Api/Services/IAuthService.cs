using L_Bank_W_Backend.Core.Models;
using L_Bank.Api.Dtos;

namespace L_Bank.Api.Services;

public interface IAuthService
{
    public string CreateJwt(User? user);
    public Task<DtoWrapper<LoginResponse>> Login(LoginRequest request);
}
