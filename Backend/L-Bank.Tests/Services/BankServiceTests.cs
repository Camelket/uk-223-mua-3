using System;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.interfaces;
using L_Bank_W_Backend.DbAccess.Interfaces;
using L_Bank_W_Backend.Interfaces;
using L_Bank.Api.Dtos;
using L_Bank.Api.Services;
using L_Bank.Core.Models;
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
    private BankService bankService;

    public BankServiceTests()
    {
        _ledgerMock = new Mock<IEFLedgerRepository>();
        _bookingMock = new Mock<IEFBookingRepository>();
        _depositMock = new Mock<IEFDepositRepository>();
        _userMock = new Mock<IEFUserRepository>();
        _loggerMock = new Mock<ILogger<BankService>>();

        bankService = new BankService(
            _bookingMock.Object,
            _userMock.Object,
            _ledgerMock.Object,
            _depositMock.Object,
            _loggerMock.Object
        );
    }

    private void Setup_LedgerBelongsToUser()
    {
        _ledgerMock.Setup(x => x.GetOne(1)).ReturnsAsync(new Ledger() { UserId = 1 });
    }

    [Fact]
    public async Task LedgerBelongsToUser_ReturnsTrue_WhenUserOwnsLedger()
    {
        Setup_LedgerBelongsToUser();
        Assert.True(await bankService.LedgerBelongsToUser(1, 1));
    }

    [Fact]
    public async Task LedgerBelongsToUser_ReturnsFalse_WhenUserDoesntOwnLedger()
    {
        Setup_LedgerBelongsToUser();
        Assert.False(await bankService.LedgerBelongsToUser(1, 2));
    }

    private void Setup_BookingBelongsToUser()
    {
        _bookingMock
            .Setup(x => x.GetOneWithLedgers(1))
            .ReturnsAsync(
                new Booking()
                {
                    Source = new Ledger() { UserId = 1 },
                    Destination = new Ledger() { UserId = 2 },
                }
            );
    }

    [Fact]
    public async Task BookingBelongsToUser_ReturnsTrue_WhenUserOwnsSourceLedger()
    {
        Setup_BookingBelongsToUser();
        Assert.True(await bankService.BookingBelongsToUser(1, 1));
    }

    [Fact]
    public async Task BookingBelongsToUser_ReturnsTrue_WhenUserOwnsTargetLedger()
    {
        Setup_BookingBelongsToUser();
        Assert.True(await bankService.BookingBelongsToUser(1, 2));
    }

    [Fact]
    public async Task BookingBelongsToUser_ReturnsFalse_WhenUserDoesntOwnSourceOrTargetLedger()
    {
        Setup_BookingBelongsToUser();
        Assert.False(await bankService.BookingBelongsToUser(1, 3));
    }

    private void Setup__Book(Ledger ledger1, Ledger ledger2, Booking booking)
    {
        _ledgerMock.Setup(x => x.GetOne(ledger1.Id)).ReturnsAsync(ledger1);
        _ledgerMock.Setup(x => x.GetOne(ledger2.Id)).ReturnsAsync(ledger2);

        _ledgerMock.Setup(x => x.Save(ledger1)).ReturnsAsync(ledger1);
        _ledgerMock.Setup(x => x.Save(ledger2)).ReturnsAsync(ledger2);
        _bookingMock.Setup(x => x.Save(booking)).ReturnsAsync(booking);
    }

    private void Setup_NegativeTest__Book()
    {
        var ledger1 = new Ledger() { Id = 1, Balance = 200 };
        var ledger2 = new Ledger() { Id = 2, Balance = 100 };

        _ledgerMock.Setup(x => x.GetOne(ledger1.Id)).ReturnsAsync(ledger1);
        _ledgerMock.Setup(x => x.GetOne(ledger2.Id)).ReturnsAsync(ledger2);

        _ledgerMock.Setup(x => x.Save(ledger1)).ReturnsAsync(ledger1);
        _ledgerMock.Setup(x => x.Save(ledger2)).ReturnsAsync(ledger2);
    }

    [Fact]
    public async Task _Book_ReturnsNotFoundTransactionResult_WhenSourceDoesnExist()
    {
        Setup_NegativeTest__Book();

        var result = await bankService._Book(99, 2, 100);
        Assert.Equal(ServiceStatus.NotFound, result.status);
        Assert.Null(result.booking);
    }

    [Fact]
    public async Task _Book_ReturnsNotFoundTransactionResult_WhenTargetDoesnExist()
    {
        Setup_NegativeTest__Book();

        var result = await bankService._Book(1, 99, 100);
        Assert.Equal(ServiceStatus.NotFound, result.status);
        Assert.Null(result.booking);
    }

    [Fact]
    public async Task _Book_ReturnsBadRequestTransactionResult_WhenAmountIsZero()
    {
        Setup_NegativeTest__Book();

        var result = await bankService._Book(1, 2, 0);
        Assert.Equal(ServiceStatus.BadRequest, result.status);
        Assert.Null(result.booking);
    }

    [Fact]
    public async Task _Book_ReturnsBadRequestTransactionResult_WhenAmountIsNegative()
    {
        Setup_NegativeTest__Book();

        var result = await bankService._Book(1, 2, -5);
        Assert.Equal(ServiceStatus.BadRequest, result.status);
        Assert.Null(result.booking);
    }

    [Fact]
    public async Task _Book_ReturnsBadRequestTransactionResult_WhenSourceBalanceIsSmallerThanAmount()
    {
        Setup_NegativeTest__Book();

        var result = await bankService._Book(1, 2, 1000);
        Assert.Equal(ServiceStatus.BadRequest, result.status);
        Assert.Null(result.booking);
    }

    [Fact]
    public async Task _Book_ReturnsSuccessTransactionResult_WhenSuppliedCorrectInputs()
    {
        decimal amount = 150;

        var ledger1 = new Ledger() { Id = 1, Balance = 200 };
        var ledger2 = new Ledger() { Id = 2, Balance = 100 };

        var booking = new Booking()
        {
            Source = ledger1,
            Destination = ledger2,
            Amount = amount,
        };

        Setup__Book(ledger1, ledger2, booking);

        var result = await bankService._Book(ledger1.Id, ledger2.Id, amount);
        Assert.Equal(ServiceStatus.Success, result.status);
        Assert.Null(result.message);

        Assert.Equal(amount, result.booking?.Amount);
        Assert.Equivalent(ledger1, result.booking?.Source);
        Assert.Equivalent(ledger2, result.booking?.Destination);

        Assert.Equal(50, ledger1.Balance);
        Assert.Equal(250, ledger2.Balance);
    }

    [Fact]
    public async Task _Book_ReturnsSuccessTransactionResult_WhenAmountIsSlightlyAboveZero()
    {
        decimal amount = 0.001M;

        var ledger1 = new Ledger() { Id = 1, Balance = 200 };
        var ledger2 = new Ledger() { Id = 2, Balance = 100 };

        var booking = new Booking()
        {
            Source = ledger1,
            Destination = ledger2,
            Amount = amount,
        };

        Setup__Book(ledger1, ledger2, booking);

        var result = await bankService._Book(ledger1.Id, ledger2.Id, amount);
        Assert.Equal(ServiceStatus.Success, result.status);
        Assert.Null(result.message);

        Assert.Equal(amount, result.booking?.Amount);
        Assert.Equivalent(ledger1, result.booking?.Source);
        Assert.Equivalent(ledger2, result.booking?.Destination);

        Assert.Equal(199.999M, ledger1.Balance);
        Assert.Equal(100.001M, ledger2.Balance);
    }

    private void Setup__DepositOrWithdrawl(Ledger ledger, Deposit deposit)
    {
        _ledgerMock.Setup(x => x.GetOne(ledger.Id)).ReturnsAsync(ledger);
        _ledgerMock.Setup(x => x.Save(ledger)).ReturnsAsync(ledger);
        _depositMock.Setup(x => x.Save(deposit)).ReturnsAsync(deposit);
    }

    private void Setup_NegativeTest__DepositOrWithdrawl()
    {
        var ledger = new Ledger() { Id = 1, Balance = 200 };

        _ledgerMock.Setup(x => x.GetOne(ledger.Id)).ReturnsAsync(ledger);
        _ledgerMock.Setup(x => x.Save(ledger)).ReturnsAsync(ledger);
    }

    [Fact]
    public async Task _DepositOrWithdrawl_ReturnsNotFoundTransactionResult_WhenLedgerDoesntExist()
    {
        Setup_NegativeTest__DepositOrWithdrawl();

        var result = await bankService._DepositOrWithdrawl(
            new DepositRequest() { LedgerId = 99, Amount = 20 },
            1
        );
        Assert.Equal(ServiceStatus.NotFound, result.status);
        Assert.Null(result.deposit);
    }

    [Fact]
    public async Task _DepositOrWithdrawl_ReturnsBadRequestTransactionResult_WhenWithdrawlAmountIsMoreThanBalance()
    {
        Setup_NegativeTest__DepositOrWithdrawl();

        var result = await bankService._DepositOrWithdrawl(
            new DepositRequest() { LedgerId = 1, Amount = -2000 },
            1
        );
        Assert.Equal(ServiceStatus.BadRequest, result.status);
        Assert.Null(result.deposit);
    }

    [Fact]
    public async Task _DepositOrWithdrawl_ReturnsSuccessTransactionResult_WhenWithdrawlIsValid()
    {
        decimal amount = -100;
        var ledger = new Ledger() { Id = 1, Balance = 200 };
        var deposit = new Deposit() { Ledger = ledger, Amount = amount };

        Setup__DepositOrWithdrawl(ledger, deposit);

        var result = await bankService._DepositOrWithdrawl(
            new DepositRequest() { LedgerId = ledger.Id, Amount = amount },
            1
        );
        Assert.Equal(ServiceStatus.Success, result.status);
        Assert.Null(result.message);

        Assert.Equal(amount, result.deposit?.Amount);
        Assert.Equivalent(ledger, result.deposit?.Ledger);

        Assert.Equal(100, ledger.Balance);
    }

    [Fact]
    public async Task _DepositOrWithdrawl_ReturnsSuccessTransactionResult_WhenDepositIsValid()
    {
        decimal amount = 100;
        var ledger = new Ledger() { Id = 1, Balance = 200 };
        var deposit = new Deposit() { Ledger = ledger, Amount = amount };

        Setup__DepositOrWithdrawl(ledger, deposit);

        var result = await bankService._DepositOrWithdrawl(
            new DepositRequest() { LedgerId = ledger.Id, Amount = amount },
            1
        );
        Assert.Equal(ServiceStatus.Success, result.status);
        Assert.Null(result.message);

        Assert.Equal(amount, result.deposit?.Amount);
        Assert.Equivalent(ledger, result.deposit?.Ledger);

        Assert.Equal(300, ledger.Balance);
    }
}
