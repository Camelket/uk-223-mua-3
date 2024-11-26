namespace L_Bank_W_Backend.Interfaces;

public interface IBookingRepository
{
    bool Book(int sourceLedgerId, int destinationLKedgerId, decimal amount);
}

public interface IEFBookingRepository
{
    bool Book(int sourceLedgerId, int destinationLKedgerId, decimal amount);
}
