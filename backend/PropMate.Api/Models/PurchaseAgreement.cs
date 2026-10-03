using PropMate.Api.Enums;

namespace PropMate.Api.Models;

public class PurchaseAgreement
{
    public int Id { get; set; }
    public int PurchaseOfferId { get; set; }
    public PurchaseOffer PurchaseOffer { get; set; } = null!;
    public int PropertyListingId { get; set; }
    public int SellerId { get; set; }
    public int BuyerId { get; set; }
    public decimal FinalPurchasePrice { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public string BuyerObligation { get; set; } = string.Empty;
    public string SellerObligation { get; set; } = string.Empty;
    public string PenaltyTerms { get; set; } = string.Empty;
    public bool BuyerConfirmed { get; set; }
    public DateTime? BuyerConfirmedAt { get; set; }
    public bool SellerConfirmed { get; set; }
    public DateTime? SellerConfirmedAt { get; set; }
    public AgreementStatus Status { get; set; } = AgreementStatus.AwaitingConfirmations;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
