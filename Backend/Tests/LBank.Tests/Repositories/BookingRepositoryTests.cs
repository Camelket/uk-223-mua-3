using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using L-Bank.Api.Services;
using L-Bank.Api.Dtos;

namespace LBank.Tests
{
    public class BankServiceTests
    {
        private readonly Mock<IBankService> _bankServiceMock;

        public BankServiceTests()
        {
            _bankServiceMock = new Mock<IBankService>();
        }

        [Fact]
        public async Task NewBooking_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var bookingRequest = new BookingRequest
            {
                LedgerId = 1,
                Amount = 100,
                Description = "Test Booking"
            };

            var bookingResponse = new BookingResponse
            {
                BookingId = 1,
                Status = "Success"
            };

            _bankServiceMock.Setup(service => service.NewBooking(bookingRequest))
                .ReturnsAsync(new DtoWrapper<BookingResponse> { Data = bookingResponse });

            var bankService = _bankServiceMock.Object;

            // Act
            var result = await bankService.NewBooking(bookingRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Success", result.Data.Status);
        }

        [Fact]
        public async Task NewBooking_InsufficientFunds_ThrowsException()
        {
            // Arrange
            var bookingRequest = new BookingRequest
            {
                LedgerId = 1,
                Amount = 1000, // Assuming 1000 exceeds available funds
                Description = "Test Booking"
            };

            _bankServiceMock.Setup(service => service.NewBooking(bookingRequest))
                .ThrowsAsync(new InvalidOperationException("Insufficient funds"));

            var bankService = _bankServiceMock.Object;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => bankService.NewBooking(bookingRequest));
        }
    }
}
