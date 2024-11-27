using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Repositories;
using L_Bank_W_Backend.Interfaces;
using L_Bank.Api.Dtos;
using L_Bank.Api.Services;
using L_Bank.Core.Helper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace L_Bank_W_Backend;

public class AuthService : IAuthService
{
    private readonly JwtSettings jwtSettings;
    private readonly IEFUserRepository userRepository;

    public AuthService(IOptions<JwtSettings> jwtSettings, IEFUserRepository userRepo)
    {
        this.jwtSettings = jwtSettings.Value;
        this.userRepository = userRepo;

        if (string.IsNullOrWhiteSpace(this.jwtSettings.PrivateKey))
        {
            throw new ArgumentNullException(nameof(this.jwtSettings.PrivateKey));
        }
    }

    public string CreateJwt(User? user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSettings.PrivateKey!);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        );

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = GenerateClaims(user),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = credentials,
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
        };

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    private static ClaimsIdentity GenerateClaims(User user)
    {
        if (user.Username == null)
        {
            throw new ArgumentNullException(nameof(user.Username));
        }

        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.Name, user.Username));
        claims.AddClaim(new Claim(ClaimTypes.UserData, user.Id.ToString()));
        claims.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
        return claims;
    }

    public async Task<DtoWrapper<LoginResponse>> Login(LoginRequest request)
    {
        {
            var user = await userRepository.GetByUsername(request.Username);
            if (user == null)
            {
                return DtoWrapper<LoginResponse>.WrapDto(
                    ServiceStatus.Failed,
                    "Unable to retrieve User"
                );
            }
            var validPassword = PasswordHelper.VerifyPassword(request.Password, user);
            if (validPassword)
            {
                return DtoWrapper<LoginResponse>.WrapDto(
                    new LoginResponse() { Token = CreateJwt(user) },
                    null
                );
            }
            return DtoWrapper<LoginResponse>.WrapDto(ServiceStatus.Failed, "Invalid password");
        }
    }
}
