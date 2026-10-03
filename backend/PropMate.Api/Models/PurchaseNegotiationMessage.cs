namespace PropMate.Api.Models;

public class PurchaseNegotiationMessage
{
    public int Id { get; set; }
    public int PurchaseOfferId { get; set; }
    public PurchaseOffer PurchaseOffer { get; set; } = null!;
    public int SenderUserId { get; set; }
    public User SenderUser { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
