using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using L_Bank.Api.Services;
using L_Bank.Api.Dtos;
using Moq;  // Für Mocking, wenn der echte Service nicht verfügbar ist


namespace LBank.Concurrent.Test
{
    public class ConcurrentTests
    {
        private readonly ITestOutputHelper output;
        private readonly IBankService _bankService;

        public ConcurrentTests(ITestOutputHelper output)
        {
            this.output = output;
            var mockService = new Mock<IBankService>();
            _bankService = mockService.Object;
        }

        [Fact]
        public async Task TestBookingParallel()
        {
            const int numberOfBookings = 1000;
            const int users = 10;

            // Implementieren Sie hier die parallelen Buchungen
            Task[] tasks = new Task[users];
            async Task UserAction(int bookingsCount, decimal startingMoney)
            {
                Random random = new Random();
                for (int i = 0; i < numberOfBookings; i++)
                {
                    
                    // Bestimmen sie zwei zufällige Ledgers
                    // Schritt 1: Zwei zufällige Ledgers bestimmen
                    int sourceLedgerId = random.Next(1, 101);
                    int targetLedgerId = random.Next(1, 101);
                    while (sourceLedgerId == targetLedgerId)
                    {
                        targetLedgerId = random.Next(1, 101);
                    }
                     // Implementieren Sie hier die parallelen Buchungen
                     // Schritt 2: Buchung durchführen
                    var bookingRequest = new BookingRequest
                    {
                        SourceId = sourceLedgerId,
                        TargetId = targetLedgerId,
                        Amount = random.Next(1, (int)startingMoney / 10)
                    };

                    try
                    {
                        var bookingResult = await _bankService.NewBooking(bookingRequest);

                        if (bookingResult.IsSuccess)
                        {
                            output.WriteLine($"Booking successful: {sourceLedgerId} -> {targetLedgerId}, Amount: {bookingRequest.Amount}");
                        }
                        else
                        {
                            output.WriteLine($"Booking failed: {bookingResult.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        output.WriteLine($"Error during booking: {ex.Message}");
                    }
                }
            }


            for (int i = 0; i < users; i++)
            {
                tasks[i] = UserAction(numberOfBookings, 1000);
            }

            await Task.WhenAll(tasks);
        }
    }
}