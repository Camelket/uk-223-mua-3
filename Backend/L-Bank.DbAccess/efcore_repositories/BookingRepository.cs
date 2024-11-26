using L_Bank_W_Backend.Models;

namespace L_Bank_W_Backend.DbAccess.efcore_repositories;

public class BookingRepository : IBookingRepository
{
    public bool Book(int sourceLedgerId, int destinationLKedgerId, decimal amount)
    {
        throw new NotImplementedException();
    }
}
