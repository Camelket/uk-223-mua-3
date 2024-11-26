namespace L_Bank_W_Backend.Core.Models
{
    public class Ledger
    {
        public const string CollectionName = "ledgers";
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Balance { get; set; }
    }
}
