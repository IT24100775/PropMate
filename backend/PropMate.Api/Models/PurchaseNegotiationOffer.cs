using PropMate.Api.Enums;

namespace PropMate.Api.Models;

public class PurchaseNegotiationOffer
{
    public int Id { get; set; }
    public int PurchaseOfferId { get; set; }
    public PurchaseOffer PurchaseOffer { get; set; } = null!;
    public int ProposedByUserId { get; set; }
    public User ProposedByUser { get; set; } = null!;
    public decimal OfferAmount { get; set; }
    public string? Conditions { get; set; }
    public NegotiationProposalStatus Status { get; set; } = NegotiationProposalStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
