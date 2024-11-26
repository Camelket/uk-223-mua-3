namespace L_Bank_W_Backend.Core.Models
{
    public class Booking
    {
        public const string CollectionName = "Booking";
        
        public int SourceId { get; set; }
        public int DestinationId { get; set; }
        public Decimal Amount { get; set; }
    }
}
