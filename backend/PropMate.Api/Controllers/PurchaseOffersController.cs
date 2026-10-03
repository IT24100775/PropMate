using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropMate.Api.DTOs.Transactions;
using PropMate.Api.Services.Interfaces;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/purchase-offers")]
[Authorize]
public class PurchaseOffersController : ControllerBase
{
    private readonly ITransactionService _service;
    public PurchaseOffersController(ITransactionService service) => _service = service;

    [HttpPost]
    [Authorize(Roles = "BuyerRenter")]
    public Task<ActionResult<PurchaseOfferResponseDto>> Create([FromBody] CreatePurchaseOfferDto dto) =>
        Handle(async () => await _service.CreatePurchaseOfferAsync(UserId(), dto));

    [HttpGet("mine")]
    [Authorize(Roles = "BuyerRenter")]
    public async Task<ActionResult<IEnumerable<PurchaseOfferResponseDto>>> Mine() =>
        Ok(await _service.GetMyPurchaseOffersAsync(UserId()));

    [HttpGet("owner")]
    [Authorize(Roles = "OwnerAgent")]
    public async Task<ActionResult<IEnumerable<PurchaseOfferResponseDto>>> OwnerInbox() =>
        Ok(await _service.GetOwnerPurchaseOffersAsync(UserId()));

    [HttpGet("{id:int}")]
    [Authorize(Roles = "BuyerRenter,OwnerAgent,Admin")]
    public async Task<ActionResult<PurchaseOfferResponseDto>> Get(int id)
    {
        try
        {
            var item = await _service.GetPurchaseOfferAsync(id, UserId(), Role());
            return item == null ? NotFound() : Ok(item);
        }
        catch (UnauthorizedAccessException ex) { return ForbidWithMessage(ex.Message); }
    }

    [HttpPost("{id:int}/accept")]
    [Authorize(Roles = "OwnerAgent")]
    public Task<ActionResult<PurchaseOfferResponseDto>> Accept(int id) =>
        Handle(async () => await _service.AcceptPurchaseOfferAsync(id, UserId()));

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "OwnerAgent")]
    public Task<ActionResult<PurchaseOfferResponseDto>> Reject(int id) =>
        Handle(async () => await _service.RejectPurchaseOfferAsync(id, UserId()));

    [HttpPost("{id:int}/negotiation/counter")]
    [Authorize(Roles = "BuyerRenter,OwnerAgent")]
    public Task<ActionResult<PurchaseNegotiationOfferResponseDto>> Counter(int id, [FromBody] PurchaseCounterOfferDto dto) =>
        Handle(async () => await _service.CreatePurchaseCounterOfferAsync(id, UserId(), Role(), dto));

    [HttpPost("{id:int}/negotiation/offers/{offerId:int}/accept")]
    [Authorize(Roles = "BuyerRenter,OwnerAgent")]
    public Task<ActionResult<PurchaseOfferResponseDto>> AcceptCounter(int id, int offerId) =>
        Handle(async () => await _service.AcceptPurchaseNegotiationOfferAsync(id, offerId, UserId(), Role()));

    [HttpGet("{id:int}/negotiation/offers")]
    [Authorize(Roles = "BuyerRenter,OwnerAgent,Admin")]
    public async Task<ActionResult<IEnumerable<PurchaseNegotiationOfferResponseDto>>> Offers(int id)
    {
        try { return Ok(await _service.GetPurchaseNegotiationOffersAsync(id, UserId(), Role())); }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { return Error(ex); }
    }

    [HttpPost("{id:int}/negotiation/messages")]
    [Authorize(Roles = "BuyerRenter,OwnerAgent")]
    public Task<ActionResult<NegotiationMessageResponseDto>> SendMessage(int id, [FromBody] NegotiationMessageDto dto) =>
        Handle(async () => await _service.AddPurchaseMessageAsync(id, UserId(), Role(), dto));

    [HttpGet("{id:int}/negotiation/messages")]
    [Authorize(Roles = "BuyerRenter,OwnerAgent,Admin")]
    public async Task<ActionResult<IEnumerable<NegotiationMessageResponseDto>>> Messages(int id)
    {
        try { return Ok(await _service.GetPurchaseMessagesAsync(id, UserId(), Role())); }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { return Error(ex); }
    }

    [HttpGet("{id:int}/agreement")]
    [Authorize(Roles = "BuyerRenter,OwnerAgent,Admin")]
    public async Task<ActionResult<PurchaseAgreementResponseDto>> Agreement(int id)
    {
        try
        {
            var agreement = await _service.GetPurchaseAgreementAsync(id, UserId(), Role());
            return agreement == null ? NotFound() : Ok(agreement);
        }
        catch (UnauthorizedAccessException ex) { return ForbidWithMessage(ex.Message); }
    }

    [HttpPost("{id:int}/agreement/confirm")]
    [Authorize(Roles = "BuyerRenter,OwnerAgent")]
    public Task<ActionResult<PurchaseAgreementResponseDto>> ConfirmAgreement(int id) =>
        Handle(async () => await _service.ConfirmPurchaseAgreementAsync(id, UserId(), Role()));

    private int UserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string Role() => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    private async Task<ActionResult<T>> Handle<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { return Error(ex); }
    }

    private ActionResult Error(Exception ex) => ex is UnauthorizedAccessException ? ForbidWithMessage(ex.Message) : BadRequest(new { message = ex.Message });
    private ObjectResult ForbidWithMessage(string message) => StatusCode(StatusCodes.Status403Forbidden, new { message });
}
