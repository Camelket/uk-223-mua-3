using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using L_Bank.Api.Services;
using L_Bank.Api.Dtos;

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
                SourceId = 1,
                TargetId = 2,
                Amount = 100

            };
            
            var bookingResponse = new BookingResponse
            {
                SourceId = 1,
                TargetId = 2,
                TransferedAmount = 1000

            };

            _bankServiceMock.Setup(service => service.NewBooking(bookingRequest))
                .ReturnsAsync(new DtoWrapper<BookingResponse> { Data = bookingResponse });

            var bankService = _bankServiceMock.Object;

            // Act
            var result = await bankService.NewBooking(bookingRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000, result.Data.TransferedAmount);
        }

        [Fact]
        public async Task NewBooking_InsufficientFunds_ThrowsException()
        {
            // Arrange
            var bookingRequest = new BookingRequest
            {
                SourceId = 1,
                TargetId = 2,
                Amount = 1000 // Assuming 1000 exceeds available funds
               
            };

            _bankServiceMock.Setup(service => service.NewBooking(bookingRequest))
                .ThrowsAsync(new InvalidOperationException("Insufficient funds"));

            var bankService = _bankServiceMock.Object;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => bankService.NewBooking(bookingRequest));
        }
    }
}
