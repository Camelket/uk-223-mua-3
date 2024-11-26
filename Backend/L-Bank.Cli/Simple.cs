using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Repositories;

namespace L_Bank.Cli;

public static class Simple
{
    public static void Run(ILedgerRepository ledgerRepository)
    {
        ////////////////////
        // Your Code Here
        ////////////////////

        Console.WriteLine();
        Console.WriteLine("Getting total money in system at the end.");
        try
        {
            decimal startMoney = ledgerRepository.GetTotalMoney();
            Console.WriteLine($"Total end money: {startMoney}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in getting total money.");
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("Hello, World!");
    }
}