using System.ComponentModel.DataAnnotations;
using PropMate.Api.Enums;

namespace PropMate.Api.DTOs.Transactions;

public class CreateRentalApplicationDto
{
    [Required]
    public int PropertyListingId { get; set; }

    [Required, MaxLength(200)]
    public string Employment { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal MonthlyIncome { get; set; }

    [Range(1, 50)]
    public int Occupants { get; set; }

    public DateOnly PreferredMoveInDate { get; set; }

    [Range(1, 120)]
    public int DurationMonths { get; set; }

    [MaxLength(2000)]
    public string? Message { get; set; }
}

public class RentalCounterOfferDto
{
    [Range(0.01, double.MaxValue)]
    public decimal MonthlyRent { get; set; }

    public DateOnly MoveInDate { get; set; }

    [Range(1, 120)]
    public int DurationMonths { get; set; }

    [MaxLength(2000)]
    public string? Conditions { get; set; }
}

public class NegotiationMessageDto
{
    [Required, MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}

public class RentalApplicationResponseDto
{
    public int Id { get; set; }
    public int PropertyListingId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Employment { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public int Occupants { get; set; }
    public DateOnly PreferredMoveInDate { get; set; }
    public int DurationMonths { get; set; }
    public string? Message { get; set; }
    public TransactionSubmissionStatus Status { get; set; }
    public NegotiationStatus NegotiationStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RentalNegotiationOfferResponseDto
{
    public int Id { get; set; }
    public int ProposedByUserId { get; set; }
    public decimal MonthlyRent { get; set; }
    public DateOnly MoveInDate { get; set; }
    public int DurationMonths { get; set; }
    public string? Conditions { get; set; }
    public NegotiationProposalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NegotiationMessageResponseDto
{
    public int Id { get; set; }
    public int SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RentalAgreementResponseDto
{
    public int Id { get; set; }
    public int RentalApplicationId { get; set; }
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
    public AgreementStatus Status { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
