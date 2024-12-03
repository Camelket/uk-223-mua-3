using System.ComponentModel.DataAnnotations.Schema;
using L_Bank.Core.Models;

namespace L_Bank_W_Backend.Core.Models
{
    public class Ledger
    {
        public const string CollectionName = "ledgers";
        public int Id { get; set; }

        public int UserId { get; set; }
        public string? Name { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        public User? User { get; set; }

        public List<Deposit> Deposits { get; set; } = [];
    }
}
