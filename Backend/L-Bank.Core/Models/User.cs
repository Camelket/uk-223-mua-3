namespace L_Bank_W_Backend.Core.Models
{
    public enum Roles
    {
        Admin,
        User,
    }

    public class User
    {
        public const string CollectionName = "users";
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public Roles Role { get; set; }

        public List<Ledger> Ledgers { get; set; } = [];
    }
}
