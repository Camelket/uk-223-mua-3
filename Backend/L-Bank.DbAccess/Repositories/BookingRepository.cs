using System.Data;
using System.Data.SqlClient;
using L_Bank_W_Backend.Models;
using Microsoft.Extensions.Options;

namespace L_Bank_W_Backend.DbAccess.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        DatabaseSettings settings;
        ILedgerRepository ledgerRepository;
        public BookingRepository(IOptions<DatabaseSettings> settings, ILedgerRepository ledgerRepository)
        {
            this.settings = settings.Value;
            this.ledgerRepository = ledgerRepository;
        }
        
        public bool Book(int sourceLedgerId, int destinationLKedgerId, decimal amount)
        {
            bool transactionWorked;
            do
            {
                transactionWorked = true;
                using (SqlConnection conn = new(settings.ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction(IsolationLevel.Serializable))
                    {
                        try
                        {
                            var sourceLedger = ledgerRepository.SelectOne(sourceLedgerId, conn, transaction);
                            if (sourceLedger == null) {
                                throw new Exception();
                            }

                            if (sourceLedger.Balance >= amount)
                            {
                                var targetLedger = ledgerRepository.SelectOne(destinationLKedgerId, conn, transaction);
                                if (targetLedger == null)
                                {
                                    throw new Exception();
                                }
                                sourceLedger.Balance -= amount;
                                targetLedger.Balance += amount;
                                ledgerRepository.Update(sourceLedger, conn, transaction);
                                ledgerRepository.Update(targetLedger, conn, transaction);
                                return true;
                            }
                            return false;
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                transaction.Rollback();
                                if (ex.GetType() != typeof(Exception)) transactionWorked = false;
                            }
                            catch (Exception rollBackEx)
                            {
                                if (rollBackEx.GetType() != typeof(Exception)) transactionWorked = false;
                            }
                        }
                    }
                }
            } while(!transactionWorked);

            // Machen Sie eine Connection und eine Transaktion

            // In der Transaktion:

            // Schauen Sie ob genügend Geld beim Spender da ist
            // Führen Sie die Buchung durch und UPDATEn Sie die ledgers
            // Beenden Sie die Transaktion
            // Bei einem Transaktionsproblem: Restarten Sie die Transaktion in einer Schleife 
            // (Siehe LedgersModel.SelectOne)

            return false; // Lösch mich
        }
    }
}

