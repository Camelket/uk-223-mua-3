namespace L_Bank.Api.Dtos;

public class BookingResponse
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public int TargetId { get; set; }
    public decimal TransferedAmount { get; set; }

    public DateTime Date { get; set; }
}
