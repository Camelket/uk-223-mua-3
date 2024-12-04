using System;
using L_Bank.Core.Models;

namespace L_Bank_W_Backend.DbAccess.interfaces;

public interface IEFDepositRepository
{
    Task<IEnumerable<Deposit>> GetAllDeposits();

    Task<IEnumerable<Deposit>> GetDepositsByLedger(int ledgerId);
    Task<IEnumerable<Deposit>> GetDepositsByUser(int userId);

    Task<Deposit> Save(Deposit deposit);
    void LockDepositTable();
}
