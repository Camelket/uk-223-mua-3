using System;
using Xunit.Abstractions;

namespace LBank.Concurrent.Test
{
    public class ConcurrentTests
    {
        private readonly ITestOutputHelper output;

        public ConcurrentTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestBookingParallel()
        {
            const int numberOfBookings = 1000;
            const int users = 10;

            // Implementieren Sie hier die parallelen Buchungen
            Task[] tasks = new Task[users];
            void UserAction(int bookingsCount, decimal startingMoney)
            {
                Random random = new Random();
                for (int i = 0; i < numberOfBookings; i++)
                {
                    // Implementieren Sie hier die parallelen Buchungen
                    // Bestimmen sie zwei zufällige Ledgers
                }
            }

            for (int i = 0; i < users; i++)
            {
                tasks[i] = Task.Run(() => UserAction(numberOfBookings, 1000));
            }

            Task.WaitAll(tasks);
    }
}