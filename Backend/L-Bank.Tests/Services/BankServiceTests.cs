using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.interfaces;
using L_Bank_W_Backend.DbAccess.Interfaces;
using L_Bank_W_Backend.Interfaces;
using L_Bank.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace L_Bank.Tests.Services;

public class BankServiceTests
{
    private readonly Mock<IEFLedgerRepository> _ledgerMock;
    private readonly Mock<IEFBookingRepository> _bookingMock;
    private readonly Mock<IEFDepositRepository> _depositMock;
    private readonly Mock<IEFUserRepository> _userMock;
    private readonly Mock<ILogger<BankService>> _loggerMock;
    private IBankService bankService;

    public BankServiceTests()
    {
        _ledgerMock = new Mock<IEFLedgerRepository>();
        _bookingMock = new Mock<IEFBookingRepository>();
        _depositMock = new Mock<IEFDepositRepository>();
        _userMock = new Mock<IEFUserRepository>();
        _loggerMock = new Mock<ILogger<BankService>>();
    }

    private void Setup()
    {
        bankService = new BankService(
            _bookingMock.Object,
            _userMock.Object,
            _ledgerMock.Object,
            _depositMock.Object,
            _loggerMock.Object
        );
    }

    public async void LedgerBelongsToUser_ReturnsTrue_WhenUserOwnsLedger()
    {
        _ledgerMock.Setup(x => x.GetOne(1)).ReturnsAsync(new Ledger() { UserId = 2 });
        Setup();

        Assert.True(await bankService.LedgerBelongsToUser(1, 2));
    }

    public void LedgerBelongsToUser_ReturnsFalse_WhenUserDoesntOwnLedger() { }

    public void LedgerBelongsToUser_ReturnsFalse_WhenUserIsNull() { }

    public void BookingBelongsToUser_ReturnsTrue_WhenUserOwnsSourceLedger() { }

    public void BookingBelongsToUser_ReturnsTrue_WhenUserOwnsTargetLedger() { }

    public void BookingBelongsToUser_ReturnsFalse_WhenUserDoesntOwnSourceOrTargetLedger() { }

    public void BookingBelongsToUser_ReturnsFalse_WhenUserIsNull() { }

    // [Fact]
    // public void Constructor_ShouldThrowArgumentNullException_WhenPrivateKeyIsNull()
    // {
    //     // Arrange
    //     _jwtSettings.PrivateKey = null;

    //     // Act & Assert
    //     Assert.Throws<ArgumentNullException>(() => new LoginService(_jwtSettingsMock.Object));
    // }

    // [Fact]
    // public void CreateJwt_ShouldThrowArgumentNullException_WhenUserIsNull()
    // {
    //     // Arrange
    //     var loginService = new LoginService(_jwtSettingsMock.Object);

    //     // Act & Assert
    //     Assert.Throws<ArgumentNullException>(() => loginService.CreateJwt(null));
    // }

    // [Fact]
    // public void CreateJwt_ShouldReturnValidJwt_WhenUserIsValid()
    // {
    //     // Arrange
    //     var loginService = new LoginService(_jwtSettingsMock.Object);
    //     var user = new User { Id = 1, Username = "testuser" };

    //     // Act
    //     var token = loginService.CreateJwt(user);

    //     // Assert
    //     Assert.NotNull(token);
    //     var handler = new JwtSecurityTokenHandler();
    //     var key = Encoding.ASCII.GetBytes(_jwtSettings.PrivateKey!);

    //     var validationParameters = new TokenValidationParameters
    //     {
    //         ValidateIssuer = true,
    //         ValidateAudience = true,
    //         ValidateLifetime = true,
    //         ValidateIssuerSigningKey = true,
    //         ValidIssuer = _jwtSettings.Issuer,
    //         ValidAudience = _jwtSettings.Audience,
    //         IssuerSigningKey = new SymmetricSecurityKey(key),
    //     };

    //     handler.ValidateToken(token, validationParameters, out var validatedToken);
    //     Assert.NotNull(validatedToken);
    //     Assert.IsType<JwtSecurityToken>(validatedToken);
    // }
}
