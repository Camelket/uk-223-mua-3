using System.IdentityModel.Tokens.Jwt;
using System.Text;
using L_Bank_W_Backend;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.Interfaces;
using L_Bank.Api.Dtos;
using L_Bank.Core.Helper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace L_Bank.Tests;

public class AuthServiceTests
{
    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    private readonly JwtSettings _jwtSettings;
    private Mock<IEFUserRepository> _userRepoMock;

    private static readonly string username = "username";
    private readonly string hashedUserpassword = PasswordHelper.HashAndSaltPassword(
        AuthServiceTests.username
    );

    private static readonly string password = username;

    public AuthServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            PrivateKey =
                "MIICWwIBAAKBgHZO8IQouqjDyY47ZDGdw9jPDVHadgfT1kP3igz5xamdVaYPHaN24UZMeSXjW9sWZzwFVbhOAGrjR0MM6APrlvv5mpy67S/K4q4D7Dvf6QySKFzwMZ99Qk10fK8tLoUlHG3qfk9+85LhL/Rnmd9FD7nz8+cYXFmz5LIaLEQATdyNAgMBAAECgYA9ng2Md34IKbiPGIWthcKb5/LC/+nbV8xPp9xBt9Dn7ybNjy/blC3uJCQwxIJxz/BChXDIxe9XvDnARTeN2yTOKrV6mUfI+VmON5gTD5hMGtWmxEsmTfu3JL0LjDe8Rfdu46w5qjX5jyDwU0ygJPqXJPRmHOQW0WN8oLIaDBxIQQJBAN66qMS2GtcgTqECjnZuuP+qrTKL4JzG+yLLNoyWJbMlF0/HatsmrFq/CkYwA806OTmCkUSm9x6mpX1wHKi4jbECQQCH+yVb67gdghmoNhc5vLgnm/efNnhUh7u07OCL3tE9EBbxZFRs17HftfEcfmtOtoyTBpf9jrOvaGjYxmxXWSedAkByZrHVCCxVHxUEAoomLsz7FTGM6ufd3x6TSomkQGLw1zZYFfe+xOh2W/XtAzCQsz09WuE+v/viVHpgKbuutcyhAkB8o8hXnBVz/rdTxti9FG1b6QstBXmASbXVHbaonkD+DoxpEMSNy5t/6b4qlvn2+T6a2VVhlXbAFhzcbewKmG7FAkEAs8z4Y1uI0Bf6ge4foXZ/2B9/pJpODnp2cbQjHomnXM861B/C+jPW3TJJN2cfbAxhCQT2NhzewaqoYzy7dpYsIQ==",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
        };
        _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        _jwtSettingsMock.Setup(x => x.Value).Returns(_jwtSettings);

        var user = new User()
        {
            Id = 1,
            Username = AuthServiceTests.username,
            PasswordHash = hashedUserpassword,
            Role = Roles.User,
        };

        _userRepoMock = new Mock<IEFUserRepository>();
        _userRepoMock.Setup(x => x.GetOne(1, false)).Returns(Task.FromResult<User?>(user));

        _userRepoMock
            .Setup(x => x.GetByUsername(user.Username))
            .Returns(Task.FromResult<User?>(user));
    }

    [Fact]
    public void CreateJwt_ShouldThrowArgumentNull_WhenPrivateKeyIsNull()
    {
        _jwtSettings.PrivateKey = null;

        Assert.Throws<ArgumentNullException>(
            () => new AuthService(_jwtSettingsMock.Object, _userRepoMock.Object)
        );
    }

    [Fact]
    public async Task CreateJwt_ShouldCreateValidToken_WhenUserIsValid()
    {
        var authService = new AuthService(_jwtSettingsMock.Object, _userRepoMock.Object);
        var user = await _userRepoMock.Object.GetOne(1, false);

        var token = authService.CreateJwt(user);

        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.PrivateKey!);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };

        handler.ValidateToken(token, validationParameters, out var validated);

        Assert.NotNull(validated);
        Assert.IsType<JwtSecurityToken>(validated);
    }

    [Fact]
    public async Task Login_ShouldReturnFailed_WhenUserIsNull()
    {
        var authService = new AuthService(_jwtSettingsMock.Object, _userRepoMock.Object);
        var response = await authService.Login(
            new Api.Dtos.LoginRequest() { Username = "", Password = "" }
        );

        Assert.NotNull(response);
        Assert.True(response.Status == ServiceStatus.Failed);
    }

    [Fact]
    public async Task Login_ShouldReturnFailed_WhenPasswordInvalid()
    {
        var authService = new AuthService(_jwtSettingsMock.Object, _userRepoMock.Object);
        var user = await _userRepoMock.Object.GetOne(1, false);

        Assert.NotNull(user);
        Assert.NotNull(user.Username);
        var response = await authService.Login(
            new Api.Dtos.LoginRequest()
            {
                Username = user.Username,
                Password = "SomeInvalidPassword",
            }
        );

        Assert.NotNull(response);
        Assert.True(response.Status == ServiceStatus.Failed);
        Assert.Contains("Invalid password", response.Message);
    }

    [Fact]
    public async Task Login_ShouldReturnSuccess_WhenPasswordValid()
    {
        var authService = new AuthService(_jwtSettingsMock.Object, _userRepoMock.Object);
        var user = await _userRepoMock.Object.GetOne(1, false);

        Assert.NotNull(user);
        Console.WriteLine(user);
        Assert.True(user.Username == AuthServiceTests.username);
        Assert.NotNull(user.PasswordHash);

        var response = await authService.Login(
            new Api.Dtos.LoginRequest()
            {
                Username = AuthServiceTests.username,
                Password = AuthServiceTests.password,
            }
        );

        Assert.NotNull(response);
        Assert.True(response.Status == ServiceStatus.Success);
    }
}
