using PropMate.Api.DTOs.Transactions;

namespace PropMate.Api.Services.Interfaces;

public interface ITransactionService
{
    Task<RentalApplicationResponseDto> CreateRentalApplicationAsync(int tenantId, CreateRentalApplicationDto dto);
    Task<IReadOnlyList<RentalApplicationResponseDto>> GetMyRentalApplicationsAsync(int tenantId);
    Task<IReadOnlyList<RentalApplicationResponseDto>> GetOwnerRentalApplicationsAsync(int ownerId);
    Task<RentalApplicationResponseDto?> GetRentalApplicationAsync(int id, int userId, string role);
    Task<RentalApplicationResponseDto> AcceptRentalApplicationAsync(int id, int ownerId);
    Task<RentalApplicationResponseDto> RejectRentalApplicationAsync(int id, int ownerId);
    Task<RentalNegotiationOfferResponseDto> CreateRentalCounterOfferAsync(int applicationId, int userId, string role, RentalCounterOfferDto dto);
    Task<RentalApplicationResponseDto> AcceptRentalNegotiationOfferAsync(int applicationId, int negotiationOfferId, int userId, string role);
    Task<IReadOnlyList<RentalNegotiationOfferResponseDto>> GetRentalNegotiationOffersAsync(int applicationId, int userId, string role);
    Task<NegotiationMessageResponseDto> AddRentalMessageAsync(int applicationId, int userId, string role, NegotiationMessageDto dto);
    Task<IReadOnlyList<NegotiationMessageResponseDto>> GetRentalMessagesAsync(int applicationId, int userId, string role);
    Task<RentalAgreementResponseDto?> GetRentalAgreementAsync(int applicationId, int userId, string role);
    Task<RentalAgreementResponseDto> ConfirmRentalAgreementAsync(int applicationId, int userId, string role);

    Task<PurchaseOfferResponseDto> CreatePurchaseOfferAsync(int buyerId, CreatePurchaseOfferDto dto);
    Task<IReadOnlyList<PurchaseOfferResponseDto>> GetMyPurchaseOffersAsync(int buyerId);
    Task<IReadOnlyList<PurchaseOfferResponseDto>> GetOwnerPurchaseOffersAsync(int ownerId);
    Task<PurchaseOfferResponseDto?> GetPurchaseOfferAsync(int id, int userId, string role);
    Task<PurchaseOfferResponseDto> AcceptPurchaseOfferAsync(int id, int ownerId);
    Task<PurchaseOfferResponseDto> RejectPurchaseOfferAsync(int id, int ownerId);
    Task<PurchaseNegotiationOfferResponseDto> CreatePurchaseCounterOfferAsync(int offerId, int userId, string role, PurchaseCounterOfferDto dto);
    Task<PurchaseOfferResponseDto> AcceptPurchaseNegotiationOfferAsync(int offerId, int negotiationOfferId, int userId, string role);
    Task<IReadOnlyList<PurchaseNegotiationOfferResponseDto>> GetPurchaseNegotiationOffersAsync(int offerId, int userId, string role);
    Task<NegotiationMessageResponseDto> AddPurchaseMessageAsync(int offerId, int userId, string role, NegotiationMessageDto dto);
    Task<IReadOnlyList<NegotiationMessageResponseDto>> GetPurchaseMessagesAsync(int offerId, int userId, string role);
    Task<PurchaseAgreementResponseDto?> GetPurchaseAgreementAsync(int offerId, int userId, string role);
    Task<PurchaseAgreementResponseDto> ConfirmPurchaseAgreementAsync(int offerId, int userId, string role);
}
