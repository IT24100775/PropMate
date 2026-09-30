using System.ComponentModel.DataAnnotations;
using PropMate.Api.Enums;

namespace PropMate.Api.DTOs.Transactions;

public class CreatePurchaseOfferDto
{
    [Required]
    public int PropertyListingId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal OfferAmount { get; set; }

    [MaxLength(3000)]
    public string? Conditions { get; set; }
}

public class PurchaseCounterOfferDto
{
    [Range(0.01, double.MaxValue)]
    public decimal OfferAmount { get; set; }

    [MaxLength(3000)]
    public string? Conditions { get; set; }
}

public class PurchaseOfferResponseDto
{
    public int Id { get; set; }
    public int PropertyListingId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public decimal OfferAmount { get; set; }
    public string? Conditions { get; set; }
    public TransactionSubmissionStatus Status { get; set; }
    public NegotiationStatus NegotiationStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PurchaseNegotiationOfferResponseDto
{
    public int Id { get; set; }
    public int ProposedByUserId { get; set; }
    public decimal OfferAmount { get; set; }
    public string? Conditions { get; set; }
    public NegotiationProposalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PurchaseAgreementResponseDto
{
    public int Id { get; set; }
    public int PurchaseOfferId { get; set; }
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
    public AgreementStatus Status { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
