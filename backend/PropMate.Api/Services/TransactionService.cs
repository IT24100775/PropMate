using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.DTOs.Transactions;
using PropMate.Api.Enums;
using PropMate.Api.Models;
using PropMate.Api.Services.Interfaces;

namespace PropMate.Api.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;

    public TransactionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RentalApplicationResponseDto> CreateRentalApplicationAsync(int tenantId, CreateRentalApplicationDto dto)
    {
        var listing = await _db.PropertyListings.FindAsync(dto.PropertyListingId)
            ?? throw new InvalidOperationException("Property listing not found.");

        if (listing.Purpose != ListingPurpose.Rent || listing.Status != ListingStatus.Published)
            throw new InvalidOperationException("Rental applications can only be submitted for published rental listings.");
        if (listing.OwnerId == tenantId)
            throw new InvalidOperationException("Owners cannot apply to rent their own listing.");
        if (dto.PreferredMoveInDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new InvalidOperationException("Preferred move-in date cannot be in the past.");

        var entity = new RentalApplication
        {
            PropertyListingId = dto.PropertyListingId,
            TenantId = tenantId,
            Employment = dto.Employment.Trim(),
            MonthlyIncome = dto.MonthlyIncome,
            Occupants = dto.Occupants,
            PreferredMoveInDate = dto.PreferredMoveInDate,
            DurationMonths = dto.DurationMonths,
            Message = dto.Message?.Trim()
        };

        _db.RentalApplications.Add(entity);
        await _db.SaveChangesAsync();
        return (await QueryRentalApplications().FirstAsync(x => x.Id == entity.Id)).ToRentalDto();
    }

    public async Task<IReadOnlyList<RentalApplicationResponseDto>> GetMyRentalApplicationsAsync(int tenantId) =>
        (await QueryRentalApplications().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).ToListAsync())
            .Select(x => x.ToRentalDto()).ToList();

    public async Task<IReadOnlyList<RentalApplicationResponseDto>> GetOwnerRentalApplicationsAsync(int ownerId) =>
        (await QueryRentalApplications().Where(x => x.PropertyListing.OwnerId == ownerId).OrderByDescending(x => x.CreatedAt).ToListAsync())
            .Select(x => x.ToRentalDto()).ToList();

    public async Task<RentalApplicationResponseDto?> GetRentalApplicationAsync(int id, int userId, string role)
    {
        var entity = await QueryRentalApplications().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return null;
        EnsureRentalAccess(entity, userId, role, allowAdmin: true);
        return entity.ToRentalDto();
    }

    public async Task<RentalApplicationResponseDto> AcceptRentalApplicationAsync(int id, int ownerId)
    {
        var entity = await QueryRentalApplications().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("Rental application not found.");
        EnsureOwner(entity.PropertyListing.OwnerId, ownerId);
        EnsureCanRespond(entity.Status);
        await CloseRentalAndGenerateAgreement(entity, entity.PropertyListing.Price, entity.PreferredMoveInDate, entity.DurationMonths, entity.Message);
        return entity.ToRentalDto();
    }

    public async Task<RentalApplicationResponseDto> RejectRentalApplicationAsync(int id, int ownerId)
    {
        var entity = await QueryRentalApplications().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("Rental application not found.");
        EnsureOwner(entity.PropertyListing.OwnerId, ownerId);
        EnsureCanRespond(entity.Status);
        entity.Status = TransactionSubmissionStatus.Rejected;
        entity.NegotiationStatus = NegotiationStatus.ClosedRejected;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return entity.ToRentalDto();
    }

    public async Task<RentalNegotiationOfferResponseDto> CreateRentalCounterOfferAsync(int applicationId, int userId, string role, RentalCounterOfferDto dto)
    {
        var entity = await QueryRentalApplications().FirstOrDefaultAsync(x => x.Id == applicationId)
            ?? throw new InvalidOperationException("Rental application not found.");
        EnsureRentalParticipant(entity, userId, role);
        EnsureNegotiationOpenable(entity.Status, entity.NegotiationStatus);
        if (dto.MoveInDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new InvalidOperationException("Move-in date cannot be in the past.");

        foreach (var pending in await _db.RentalNegotiationOffers.Where(x => x.RentalApplicationId == applicationId && x.Status == NegotiationProposalStatus.Pending).ToListAsync())
            pending.Status = NegotiationProposalStatus.Superseded;

        var offer = new RentalNegotiationOffer
        {
            RentalApplicationId = applicationId,
            ProposedByUserId = userId,
            MonthlyRent = dto.MonthlyRent,
            MoveInDate = dto.MoveInDate,
            DurationMonths = dto.DurationMonths,
            Conditions = dto.Conditions?.Trim()
        };
        entity.Status = TransactionSubmissionStatus.InNegotiation;
        entity.NegotiationStatus = NegotiationStatus.Open;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.RentalNegotiationOffers.Add(offer);
        await _db.SaveChangesAsync();
        return offer.ToDto();
    }

    public async Task<RentalApplicationResponseDto> AcceptRentalNegotiationOfferAsync(int applicationId, int negotiationOfferId, int userId, string role)
    {
        var entity = await QueryRentalApplications().FirstOrDefaultAsync(x => x.Id == applicationId)
            ?? throw new InvalidOperationException("Rental application not found.");
        EnsureRentalParticipant(entity, userId, role);
        var offer = await _db.RentalNegotiationOffers.FirstOrDefaultAsync(x => x.Id == negotiationOfferId && x.RentalApplicationId == applicationId)
            ?? throw new InvalidOperationException("Negotiation offer not found.");
        if (offer.Status != NegotiationProposalStatus.Pending)
            throw new InvalidOperationException("Only a pending negotiation offer can be accepted.");
        if (offer.ProposedByUserId == userId)
            throw new InvalidOperationException("A user cannot accept their own counter-offer.");

        offer.Status = NegotiationProposalStatus.Accepted;
        foreach (var other in await _db.RentalNegotiationOffers.Where(x => x.RentalApplicationId == applicationId && x.Id != offer.Id && x.Status == NegotiationProposalStatus.Pending).ToListAsync())
            other.Status = NegotiationProposalStatus.Superseded;
        await CloseRentalAndGenerateAgreement(entity, offer.MonthlyRent, offer.MoveInDate, offer.DurationMonths, offer.Conditions);
        return entity.ToRentalDto();
    }

    public async Task<IReadOnlyList<RentalNegotiationOfferResponseDto>> GetRentalNegotiationOffersAsync(int applicationId, int userId, string role)
    {
        var entity = await QueryRentalApplications().FirstOrDefaultAsync(x => x.Id == applicationId)
            ?? throw new InvalidOperationException("Rental application not found.");
        EnsureRentalAccess(entity, userId, role, allowAdmin: true);
        return (await _db.RentalNegotiationOffers.Where(x => x.RentalApplicationId == applicationId).OrderBy(x => x.CreatedAt).ToListAsync())
            .Select(x => x.ToDto()).ToList();
    }

    public async Task<NegotiationMessageResponseDto> AddRentalMessageAsync(int applicationId, int userId, string role, NegotiationMessageDto dto)
    {
        var entity = await QueryRentalApplications().FirstOrDefaultAsync(x => x.Id == applicationId)
            ?? throw new InvalidOperationException("Rental application not found.");
        EnsureRentalParticipant(entity, userId, role);
        if (entity.NegotiationStatus != NegotiationStatus.Open)
            throw new InvalidOperationException("Messages can only be sent while negotiation is open.");
        var msg = new RentalNegotiationMessage { RentalApplicationId = applicationId, SenderUserId = userId, Message = dto.Message.Trim() };
        _db.RentalNegotiationMessages.Add(msg);
        await _db.SaveChangesAsync();
        var sender = await _db.Users.FindAsync(userId);
        return new NegotiationMessageResponseDto { Id = msg.Id, SenderUserId = userId, SenderName = sender == null ? string.Empty : $"{sender.FirstName} {sender.LastName}", Message = msg.Message, CreatedAt = msg.CreatedAt };
    }

    public async Task<IReadOnlyList<NegotiationMessageResponseDto>> GetRentalMessagesAsync(int applicationId, int userId, string role)
    {
        var entity = await QueryRentalApplications().FirstOrDefaultAsync(x => x.Id == applicationId)
            ?? throw new InvalidOperationException("Rental application not found.");
        EnsureRentalAccess(entity, userId, role, allowAdmin: true);
        return await _db.RentalNegotiationMessages.Where(x => x.RentalApplicationId == applicationId).Include(x => x.SenderUser).OrderBy(x => x.CreatedAt)
            .Select(x => new NegotiationMessageResponseDto { Id = x.Id, SenderUserId = x.SenderUserId, SenderName = x.SenderUser.FirstName + " " + x.SenderUser.LastName, Message = x.Message, CreatedAt = x.CreatedAt }).ToListAsync();
    }

    public async Task<RentalAgreementResponseDto?> GetRentalAgreementAsync(int applicationId, int userId, string role)
    {
        var entity = await QueryRentalApplications().FirstOrDefaultAsync(x => x.Id == applicationId);
        if (entity == null) return null;
        EnsureRentalAccess(entity, userId, role, allowAdmin: true);
        var agreement = await _db.RentalAgreements.FirstOrDefaultAsync(x => x.RentalApplicationId == applicationId);
        return agreement?.ToDto();
    }

    public async Task<RentalAgreementResponseDto> ConfirmRentalAgreementAsync(int applicationId, int userId, string role)
    {
        var entity = await QueryRentalApplications().FirstOrDefaultAsync(x => x.Id == applicationId)
            ?? throw new InvalidOperationException("Rental application not found.");
        EnsureRentalParticipant(entity, userId, role);
        var agreement = await _db.RentalAgreements.FirstOrDefaultAsync(x => x.RentalApplicationId == applicationId)
            ?? throw new InvalidOperationException("Rental agreement has not been generated.");
        var now = DateTime.UtcNow;
        if (userId == entity.TenantId) { agreement.BuyerConfirmed = true; agreement.BuyerConfirmedAt ??= now; }
        else { agreement.SellerConfirmed = true; agreement.SellerConfirmedAt ??= now; }
        await CompleteRentalAgreementIfReady(entity, agreement, now, userId);
        await _db.SaveChangesAsync();
        return agreement.ToDto();
    }

    public async Task<PurchaseOfferResponseDto> CreatePurchaseOfferAsync(int buyerId, CreatePurchaseOfferDto dto)
    {
        var listing = await _db.PropertyListings.FindAsync(dto.PropertyListingId)
            ?? throw new InvalidOperationException("Property listing not found.");
        if (listing.Purpose != ListingPurpose.Sale || listing.Status != ListingStatus.Published)
            throw new InvalidOperationException("Purchase offers can only be submitted for published sale listings.");
        if (listing.OwnerId == buyerId)
            throw new InvalidOperationException("Owners cannot submit purchase offers on their own listing.");
        var entity = new PurchaseOffer { PropertyListingId = dto.PropertyListingId, BuyerId = buyerId, OfferAmount = dto.OfferAmount, Conditions = dto.Conditions?.Trim() };
        _db.PurchaseOffers.Add(entity);
        await _db.SaveChangesAsync();
        return (await QueryPurchaseOffers().FirstAsync(x => x.Id == entity.Id)).ToPurchaseDto();
    }

    public async Task<IReadOnlyList<PurchaseOfferResponseDto>> GetMyPurchaseOffersAsync(int buyerId) =>
        (await QueryPurchaseOffers().Where(x => x.BuyerId == buyerId).OrderByDescending(x => x.CreatedAt).ToListAsync()).Select(x => x.ToPurchaseDto()).ToList();

    public async Task<IReadOnlyList<PurchaseOfferResponseDto>> GetOwnerPurchaseOffersAsync(int ownerId) =>
        (await QueryPurchaseOffers().Where(x => x.PropertyListing.OwnerId == ownerId).OrderByDescending(x => x.CreatedAt).ToListAsync()).Select(x => x.ToPurchaseDto()).ToList();

    public async Task<PurchaseOfferResponseDto?> GetPurchaseOfferAsync(int id, int userId, string role)
    {
        var entity = await QueryPurchaseOffers().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return null;
        EnsurePurchaseAccess(entity, userId, role, allowAdmin: true);
        return entity.ToPurchaseDto();
    }

    public async Task<PurchaseOfferResponseDto> AcceptPurchaseOfferAsync(int id, int ownerId)
    {
        var entity = await QueryPurchaseOffers().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("Purchase offer not found.");
        EnsureOwner(entity.PropertyListing.OwnerId, ownerId);
        EnsureCanRespond(entity.Status);
        await ClosePurchaseAndGenerateAgreement(entity, entity.OfferAmount, entity.Conditions);
        return entity.ToPurchaseDto();
    }

    public async Task<PurchaseOfferResponseDto> RejectPurchaseOfferAsync(int id, int ownerId)
    {
        var entity = await QueryPurchaseOffers().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("Purchase offer not found.");
        EnsureOwner(entity.PropertyListing.OwnerId, ownerId);
        EnsureCanRespond(entity.Status);
        entity.Status = TransactionSubmissionStatus.Rejected;
        entity.NegotiationStatus = NegotiationStatus.ClosedRejected;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return entity.ToPurchaseDto();
    }

    public async Task<PurchaseNegotiationOfferResponseDto> CreatePurchaseCounterOfferAsync(int offerId, int userId, string role, PurchaseCounterOfferDto dto)
    {
        var entity = await QueryPurchaseOffers().FirstOrDefaultAsync(x => x.Id == offerId)
            ?? throw new InvalidOperationException("Purchase offer not found.");
        EnsurePurchaseParticipant(entity, userId, role);
        EnsureNegotiationOpenable(entity.Status, entity.NegotiationStatus);
        foreach (var pending in await _db.PurchaseNegotiationOffers.Where(x => x.PurchaseOfferId == offerId && x.Status == NegotiationProposalStatus.Pending).ToListAsync())
            pending.Status = NegotiationProposalStatus.Superseded;
        var offer = new PurchaseNegotiationOffer { PurchaseOfferId = offerId, ProposedByUserId = userId, OfferAmount = dto.OfferAmount, Conditions = dto.Conditions?.Trim() };
        entity.Status = TransactionSubmissionStatus.InNegotiation;
        entity.NegotiationStatus = NegotiationStatus.Open;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.PurchaseNegotiationOffers.Add(offer);
        await _db.SaveChangesAsync();
        return offer.ToDto();
    }

    public async Task<PurchaseOfferResponseDto> AcceptPurchaseNegotiationOfferAsync(int offerId, int negotiationOfferId, int userId, string role)
    {
        var entity = await QueryPurchaseOffers().FirstOrDefaultAsync(x => x.Id == offerId)
            ?? throw new InvalidOperationException("Purchase offer not found.");
        EnsurePurchaseParticipant(entity, userId, role);
        var offer = await _db.PurchaseNegotiationOffers.FirstOrDefaultAsync(x => x.Id == negotiationOfferId && x.PurchaseOfferId == offerId)
            ?? throw new InvalidOperationException("Negotiation offer not found.");
        if (offer.Status != NegotiationProposalStatus.Pending)
            throw new InvalidOperationException("Only a pending negotiation offer can be accepted.");
        if (offer.ProposedByUserId == userId)
            throw new InvalidOperationException("A user cannot accept their own counter-offer.");
        offer.Status = NegotiationProposalStatus.Accepted;
        foreach (var other in await _db.PurchaseNegotiationOffers.Where(x => x.PurchaseOfferId == offerId && x.Id != offer.Id && x.Status == NegotiationProposalStatus.Pending).ToListAsync())
            other.Status = NegotiationProposalStatus.Superseded;
        await ClosePurchaseAndGenerateAgreement(entity, offer.OfferAmount, offer.Conditions);
        return entity.ToPurchaseDto();
    }

    public async Task<IReadOnlyList<PurchaseNegotiationOfferResponseDto>> GetPurchaseNegotiationOffersAsync(int offerId, int userId, string role)
    {
        var entity = await QueryPurchaseOffers().FirstOrDefaultAsync(x => x.Id == offerId)
            ?? throw new InvalidOperationException("Purchase offer not found.");
        EnsurePurchaseAccess(entity, userId, role, allowAdmin: true);
        return (await _db.PurchaseNegotiationOffers.Where(x => x.PurchaseOfferId == offerId).OrderBy(x => x.CreatedAt).ToListAsync()).Select(x => x.ToDto()).ToList();
    }

    public async Task<NegotiationMessageResponseDto> AddPurchaseMessageAsync(int offerId, int userId, string role, NegotiationMessageDto dto)
    {
        var entity = await QueryPurchaseOffers().FirstOrDefaultAsync(x => x.Id == offerId)
            ?? throw new InvalidOperationException("Purchase offer not found.");
        EnsurePurchaseParticipant(entity, userId, role);
        if (entity.NegotiationStatus != NegotiationStatus.Open)
            throw new InvalidOperationException("Messages can only be sent while negotiation is open.");
        var msg = new PurchaseNegotiationMessage { PurchaseOfferId = offerId, SenderUserId = userId, Message = dto.Message.Trim() };
        _db.PurchaseNegotiationMessages.Add(msg);
        await _db.SaveChangesAsync();
        var sender = await _db.Users.FindAsync(userId);
        return new NegotiationMessageResponseDto { Id = msg.Id, SenderUserId = userId, SenderName = sender == null ? string.Empty : $"{sender.FirstName} {sender.LastName}", Message = msg.Message, CreatedAt = msg.CreatedAt };
    }

    public async Task<IReadOnlyList<NegotiationMessageResponseDto>> GetPurchaseMessagesAsync(int offerId, int userId, string role)
    {
        var entity = await QueryPurchaseOffers().FirstOrDefaultAsync(x => x.Id == offerId)
            ?? throw new InvalidOperationException("Purchase offer not found.");
        EnsurePurchaseAccess(entity, userId, role, allowAdmin: true);
        return await _db.PurchaseNegotiationMessages.Where(x => x.PurchaseOfferId == offerId).Include(x => x.SenderUser).OrderBy(x => x.CreatedAt)
            .Select(x => new NegotiationMessageResponseDto { Id = x.Id, SenderUserId = x.SenderUserId, SenderName = x.SenderUser.FirstName + " " + x.SenderUser.LastName, Message = x.Message, CreatedAt = x.CreatedAt }).ToListAsync();
    }

    public async Task<PurchaseAgreementResponseDto?> GetPurchaseAgreementAsync(int offerId, int userId, string role)
    {
        var entity = await QueryPurchaseOffers().FirstOrDefaultAsync(x => x.Id == offerId);
        if (entity == null) return null;
        EnsurePurchaseAccess(entity, userId, role, allowAdmin: true);
        var agreement = await _db.PurchaseAgreements.FirstOrDefaultAsync(x => x.PurchaseOfferId == offerId);
        return agreement?.ToDto();
    }

    public async Task<PurchaseAgreementResponseDto> ConfirmPurchaseAgreementAsync(int offerId, int userId, string role)
    {
        var entity = await QueryPurchaseOffers().FirstOrDefaultAsync(x => x.Id == offerId)
            ?? throw new InvalidOperationException("Purchase offer not found.");
        EnsurePurchaseParticipant(entity, userId, role);
        var agreement = await _db.PurchaseAgreements.FirstOrDefaultAsync(x => x.PurchaseOfferId == offerId)
            ?? throw new InvalidOperationException("Purchase agreement has not been generated.");
        var now = DateTime.UtcNow;
        if (userId == entity.BuyerId) { agreement.BuyerConfirmed = true; agreement.BuyerConfirmedAt ??= now; }
        else { agreement.SellerConfirmed = true; agreement.SellerConfirmedAt ??= now; }
        if (agreement.BuyerConfirmed && agreement.SellerConfirmed)
        {
            agreement.Status = AgreementStatus.Completed;
            agreement.CompletedAt ??= now;
            entity.Status = TransactionSubmissionStatus.Completed;
            var previous = entity.PropertyListing.Status;
            entity.PropertyListing.Status = ListingStatus.Sold;
            entity.PropertyListing.UpdatedAt = now;
            _db.ListingStatusHistories.Add(new ListingStatusHistory
            {
                PropertyListingId = entity.PropertyListingId,
                PreviousStatus = previous,
                NewStatus = ListingStatus.Sold,
                Reason = "Purchase agreement confirmed by buyer and seller.",
                ChangedByUserId = userId,
                ChangedAt = now
            });
        }
        else agreement.Status = agreement.BuyerConfirmed ? AgreementStatus.BuyerConfirmed : AgreementStatus.SellerConfirmed;
        await _db.SaveChangesAsync();
        return agreement.ToDto();
    }

    private IQueryable<RentalApplication> QueryRentalApplications() => _db.RentalApplications.Include(x => x.PropertyListing).Include(x => x.Tenant);
    private IQueryable<PurchaseOffer> QueryPurchaseOffers() => _db.PurchaseOffers.Include(x => x.PropertyListing).Include(x => x.Buyer);

    private static void EnsureOwner(int actualOwnerId, int userId)
    {
        if (actualOwnerId != userId) throw new UnauthorizedAccessException("Only the owner of this listing can perform this action.");
    }

    private static void EnsureCanRespond(TransactionSubmissionStatus status)
    {
        if (status is TransactionSubmissionStatus.Accepted or TransactionSubmissionStatus.Rejected or TransactionSubmissionStatus.AgreementGenerated or TransactionSubmissionStatus.Completed)
            throw new InvalidOperationException("This submission is already closed.");
    }

    private static void EnsureNegotiationOpenable(TransactionSubmissionStatus status, NegotiationStatus negotiationStatus)
    {
        if (negotiationStatus is NegotiationStatus.ClosedAccepted or NegotiationStatus.ClosedRejected || status is TransactionSubmissionStatus.AgreementGenerated or TransactionSubmissionStatus.Completed or TransactionSubmissionStatus.Rejected)
            throw new InvalidOperationException("Negotiation is closed.");
    }

    private static void EnsureRentalParticipant(RentalApplication entity, int userId, string role)
    {
        if (role == UserRole.BuyerRenter.ToString() && entity.TenantId == userId) return;
        if (role == UserRole.OwnerAgent.ToString() && entity.PropertyListing.OwnerId == userId) return;
        throw new UnauthorizedAccessException("Only the tenant or listing owner can modify this rental negotiation.");
    }

    private static void EnsureRentalAccess(RentalApplication entity, int userId, string role, bool allowAdmin)
    {
        if (allowAdmin && role == UserRole.Admin.ToString()) return;
        EnsureRentalParticipant(entity, userId, role);
    }

    private static void EnsurePurchaseParticipant(PurchaseOffer entity, int userId, string role)
    {
        if (role == UserRole.BuyerRenter.ToString() && entity.BuyerId == userId) return;
        if (role == UserRole.OwnerAgent.ToString() && entity.PropertyListing.OwnerId == userId) return;
        throw new UnauthorizedAccessException("Only the buyer or listing owner can modify this purchase negotiation.");
    }

    private static void EnsurePurchaseAccess(PurchaseOffer entity, int userId, string role, bool allowAdmin)
    {
        if (allowAdmin && role == UserRole.Admin.ToString()) return;
        EnsurePurchaseParticipant(entity, userId, role);
    }

    private async Task CloseRentalAndGenerateAgreement(RentalApplication entity, decimal rent, DateOnly moveInDate, int durationMonths, string? terms)
    {
        if (await _db.RentalAgreements.AnyAsync(x => x.RentalApplicationId == entity.Id))
            throw new InvalidOperationException("An agreement has already been generated.");
        entity.Status = TransactionSubmissionStatus.AgreementGenerated;
        entity.NegotiationStatus = NegotiationStatus.ClosedAccepted;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.RentalAgreements.Add(new RentalAgreement
        {
            RentalApplicationId = entity.Id,
            PropertyListingId = entity.PropertyListingId,
            OwnerId = entity.PropertyListing.OwnerId,
            TenantId = entity.TenantId,
            FinalMonthlyRent = rent,
            MoveInDate = moveInDate,
            DurationMonths = durationMonths,
            Terms = terms ?? string.Empty,
            TenantObligation = "The tenant agrees to rent and occupy the property according to the confirmed agreement terms and to meet all agreed payment and tenancy obligations.",
            OwnerObligation = "The owner agrees to rent the property to the named tenant according to the confirmed agreement terms.",
            PenaltyTerms = "If either party fails to perform an agreed obligation, any penalty or remedy must be one expressly recorded in the confirmed agreement terms and permitted by applicable law."
        });
        await _db.SaveChangesAsync();
    }

    private async Task ClosePurchaseAndGenerateAgreement(PurchaseOffer entity, decimal amount, string? conditions)
    {
        if (await _db.PurchaseAgreements.AnyAsync(x => x.PurchaseOfferId == entity.Id))
            throw new InvalidOperationException("An agreement has already been generated.");
        entity.Status = TransactionSubmissionStatus.AgreementGenerated;
        entity.NegotiationStatus = NegotiationStatus.ClosedAccepted;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.PurchaseAgreements.Add(new PurchaseAgreement
        {
            PurchaseOfferId = entity.Id,
            PropertyListingId = entity.PropertyListingId,
            SellerId = entity.PropertyListing.OwnerId,
            BuyerId = entity.BuyerId,
            FinalPurchasePrice = amount,
            Conditions = conditions ?? string.Empty,
            BuyerObligation = "The buyer is required to purchase the identified property from the seller according to the confirmed agreement terms.",
            SellerObligation = "The seller is required to sell the identified property to the named buyer according to the confirmed agreement terms.",
            PenaltyTerms = "If either party fails to complete an agreed obligation, penalties or remedies may apply only as expressly recorded in the confirmed agreement conditions and as permitted by applicable law."
        });
        await _db.SaveChangesAsync();
    }

    private Task CompleteRentalAgreementIfReady(RentalApplication entity, RentalAgreement agreement, DateTime now, int changedByUserId)
    {
        if (agreement.BuyerConfirmed && agreement.SellerConfirmed)
        {
            agreement.Status = AgreementStatus.Completed;
            agreement.CompletedAt ??= now;
            entity.Status = TransactionSubmissionStatus.Completed;
            var previous = entity.PropertyListing.Status;
            entity.PropertyListing.Status = ListingStatus.Rented;
            entity.PropertyListing.UpdatedAt = now;
            _db.ListingStatusHistories.Add(new ListingStatusHistory
            {
                PropertyListingId = entity.PropertyListingId,
                PreviousStatus = previous,
                NewStatus = ListingStatus.Rented,
                Reason = "Rental agreement confirmed by tenant and owner.",
                ChangedByUserId = changedByUserId,
                ChangedAt = now
            });
        }
        else agreement.Status = agreement.BuyerConfirmed ? AgreementStatus.BuyerConfirmed : AgreementStatus.SellerConfirmed;
        return Task.CompletedTask;
    }
}

internal static class TransactionMappings
{
    public static RentalApplicationResponseDto ToRentalDto(this RentalApplication x) => new()
    {
        Id = x.Id, PropertyListingId = x.PropertyListingId, PropertyTitle = x.PropertyListing?.Title ?? string.Empty,
        TenantId = x.TenantId, TenantName = x.Tenant == null ? string.Empty : $"{x.Tenant.FirstName} {x.Tenant.LastName}",
        Employment = x.Employment, MonthlyIncome = x.MonthlyIncome, Occupants = x.Occupants,
        PreferredMoveInDate = x.PreferredMoveInDate, DurationMonths = x.DurationMonths, Message = x.Message,
        Status = x.Status, NegotiationStatus = x.NegotiationStatus, CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    public static PurchaseOfferResponseDto ToPurchaseDto(this PurchaseOffer x) => new()
    {
        Id = x.Id, PropertyListingId = x.PropertyListingId, PropertyTitle = x.PropertyListing?.Title ?? string.Empty,
        BuyerId = x.BuyerId, BuyerName = x.Buyer == null ? string.Empty : $"{x.Buyer.FirstName} {x.Buyer.LastName}",
        OfferAmount = x.OfferAmount, Conditions = x.Conditions, Status = x.Status, NegotiationStatus = x.NegotiationStatus,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    public static RentalNegotiationOfferResponseDto ToDto(this RentalNegotiationOffer x) => new()
    { Id = x.Id, ProposedByUserId = x.ProposedByUserId, MonthlyRent = x.MonthlyRent, MoveInDate = x.MoveInDate, DurationMonths = x.DurationMonths, Conditions = x.Conditions, Status = x.Status, CreatedAt = x.CreatedAt };

    public static PurchaseNegotiationOfferResponseDto ToDto(this PurchaseNegotiationOffer x) => new()
    { Id = x.Id, ProposedByUserId = x.ProposedByUserId, OfferAmount = x.OfferAmount, Conditions = x.Conditions, Status = x.Status, CreatedAt = x.CreatedAt };

    public static RentalAgreementResponseDto ToDto(this RentalAgreement x) => new()
    {
        Id = x.Id, RentalApplicationId = x.RentalApplicationId, PropertyListingId = x.PropertyListingId, OwnerId = x.OwnerId, TenantId = x.TenantId,
        FinalMonthlyRent = x.FinalMonthlyRent, MoveInDate = x.MoveInDate, DurationMonths = x.DurationMonths, Terms = x.Terms,
        TenantObligation = x.TenantObligation, OwnerObligation = x.OwnerObligation, PenaltyTerms = x.PenaltyTerms,
        BuyerConfirmed = x.BuyerConfirmed, BuyerConfirmedAt = x.BuyerConfirmedAt, SellerConfirmed = x.SellerConfirmed,
        SellerConfirmedAt = x.SellerConfirmedAt, Status = x.Status, GeneratedAt = x.GeneratedAt, CompletedAt = x.CompletedAt
    };

    public static PurchaseAgreementResponseDto ToDto(this PurchaseAgreement x) => new()
    {
        Id = x.Id, PurchaseOfferId = x.PurchaseOfferId, PropertyListingId = x.PropertyListingId, SellerId = x.SellerId, BuyerId = x.BuyerId,
        FinalPurchasePrice = x.FinalPurchasePrice, Conditions = x.Conditions, BuyerObligation = x.BuyerObligation,
        SellerObligation = x.SellerObligation, PenaltyTerms = x.PenaltyTerms, BuyerConfirmed = x.BuyerConfirmed,
        BuyerConfirmedAt = x.BuyerConfirmedAt, SellerConfirmed = x.SellerConfirmed, SellerConfirmedAt = x.SellerConfirmedAt,
        Status = x.Status, GeneratedAt = x.GeneratedAt, CompletedAt = x.CompletedAt
    };
}
