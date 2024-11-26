using System;
using L_Bank_W_Backend.Interfaces;

namespace L_Bank_W_Backend.DbAccess.EFRepositories;

public class EFBookingRepository(AppDbContext context) : IEFBookingRepository
{
    private readonly AppDbContext context = context;

    public bool Book(int sourceLedgerId, int destinationLKedgerId, decimal amount)
    {
        throw new NotImplementedException();
    }
}
