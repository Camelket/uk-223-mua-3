namespace L_Bank_W_Backend.Core.Models
{
    public enum Roles { Administrators, Users }

    public class User
    {
        public const string CollectionName = "users";
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public Roles Role { get; set; }
     }
}
