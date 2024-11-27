using System.ComponentModel.DataAnnotations.Schema;

namespace L_Bank_W_Backend.Core.Models
{
    public class Booking
    {
        public const string CollectionName = "Booking";

        public int Id { get; set; }
        public int SourceId { get; set; }
        public int DestinationId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public Ledger? Source { get; set; }
        public Ledger? Destination { get; set; }
    }
}
