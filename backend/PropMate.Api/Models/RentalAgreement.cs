using PropMate.Api.Enums;

namespace PropMate.Api.Models;

public class RentalAgreement
{
    public int Id { get; set; }
    public int RentalApplicationId { get; set; }
    public RentalApplication RentalApplication { get; set; } = null!;
    public int PropertyListingId { get; set; }
    public int OwnerId { get; set; }
    public int TenantId { get; set; }
    public decimal FinalMonthlyRent { get; set; }
    public DateOnly MoveInDate { get; set; }
    public int DurationMonths { get; set; }
    public string Terms { get; set; } = string.Empty;
    public string TenantObligation { get; set; } = string.Empty;
    public string OwnerObligation { get; set; } = string.Empty;
    public string PenaltyTerms { get; set; } = string.Empty;
    public bool BuyerConfirmed { get; set; }
    public DateTime? BuyerConfirmedAt { get; set; }
    public bool SellerConfirmed { get; set; }
    public DateTime? SellerConfirmedAt { get; set; }
    public AgreementStatus Status { get; set; } = AgreementStatus.AwaitingConfirmations;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
