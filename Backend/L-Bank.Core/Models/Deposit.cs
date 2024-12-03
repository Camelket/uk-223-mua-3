using System.ComponentModel.DataAnnotations.Schema;
using L_Bank_W_Backend.Core.Models;

namespace L_Bank.Core.Models;

public class Deposit
{
    public const string CollectionName = "deposits";
    public int Id { get; set; }
    public int LedgerId { get; set; }

    public int DepositorId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public DateTime date { get; set; }

    public Ledger? Ledger { get; set; }
    public User? Depositor { get; set; }
}
