namespace L_Bank.Api.Dtos;

public class UserResponse
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public List<LedgerResponse> Ledgers { get; set; } = [];
}
