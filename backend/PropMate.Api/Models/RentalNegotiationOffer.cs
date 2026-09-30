using PropMate.Api.Enums;

namespace PropMate.Api.Models;

public class RentalNegotiationOffer
{
    public int Id { get; set; }
    public int RentalApplicationId { get; set; }
    public RentalApplication RentalApplication { get; set; } = null!;
    public int ProposedByUserId { get; set; }
    public User ProposedByUser { get; set; } = null!;
    public decimal MonthlyRent { get; set; }
    public DateOnly MoveInDate { get; set; }
    public int DurationMonths { get; set; }
    public string? Conditions { get; set; }
    public NegotiationProposalStatus Status { get; set; } = NegotiationProposalStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
