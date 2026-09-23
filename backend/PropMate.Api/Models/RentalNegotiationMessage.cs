namespace PropMate.Api.Models;

public class RentalNegotiationMessage
{
    public int Id { get; set; }
    public int RentalApplicationId { get; set; }
    public RentalApplication RentalApplication { get; set; } = null!;
    public int SenderUserId { get; set; }
    public User SenderUser { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
