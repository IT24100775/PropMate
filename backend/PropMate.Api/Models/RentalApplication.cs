using PropMate.Api.Enums;

namespace PropMate.Api.Models;

public class RentalApplication
{
    public int Id { get; set; }
    public int PropertyListingId { get; set; }
    public PropertyListing PropertyListing { get; set; } = null!;
    public int TenantId { get; set; }
    public User Tenant { get; set; } = null!;
    public string Employment { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public int Occupants { get; set; }
    public DateOnly PreferredMoveInDate { get; set; }
    public int DurationMonths { get; set; }
    public string? Message { get; set; }
    public TransactionSubmissionStatus Status { get; set; } = TransactionSubmissionStatus.Pending;
    public NegotiationStatus NegotiationStatus { get; set; } = NegotiationStatus.NotStarted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RentalNegotiationOffer> NegotiationOffers { get; set; } = new List<RentalNegotiationOffer>();
    public ICollection<RentalNegotiationMessage> NegotiationMessages { get; set; } = new List<RentalNegotiationMessage>();
    public RentalAgreement? Agreement { get; set; }
}
