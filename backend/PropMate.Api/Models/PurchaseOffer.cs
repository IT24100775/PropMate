using PropMate.Api.Enums;

namespace PropMate.Api.Models;

public class PurchaseOffer
{
    public int Id { get; set; }
    public int PropertyListingId { get; set; }
    public PropertyListing PropertyListing { get; set; } = null!;
    public int BuyerId { get; set; }
    public User Buyer { get; set; } = null!;
    public decimal OfferAmount { get; set; }
    public string? Conditions { get; set; }
    public TransactionSubmissionStatus Status { get; set; } = TransactionSubmissionStatus.Pending;
    public NegotiationStatus NegotiationStatus { get; set; } = NegotiationStatus.NotStarted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PurchaseNegotiationOffer> NegotiationOffers { get; set; } = new List<PurchaseNegotiationOffer>();
    public ICollection<PurchaseNegotiationMessage> NegotiationMessages { get; set; } = new List<PurchaseNegotiationMessage>();
    public PurchaseAgreement? Agreement { get; set; }
}
