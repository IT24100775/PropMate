using Microsoft.AspNetCore.Mvc;
using PropMate.Api.DTOs.Transactions;
using PropMate.Api.Services;
using PropMate.Api.Services.Interfaces;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/purchase-offers")]
public class PurchaseOffersController : ControllerBase
{
    private readonly ITransactionService _service;
    public PurchaseOffersController(ITransactionService service) => _service = service;

    [HttpPost]
    public Task<ActionResult<PurchaseOfferResponseDto>> Create([FromBody] CreatePurchaseOfferDto dto) =>
        Handle(async () => await _service.CreatePurchaseOfferAsync(DemoIdentity.TenantId, dto));

    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<PurchaseOfferResponseDto>>> Mine() =>
        Ok(await _service.GetMyPurchaseOffersAsync(DemoIdentity.TenantId));

    [HttpGet("owner")]
    public async Task<ActionResult<IEnumerable<PurchaseOfferResponseDto>>> OwnerInbox() =>
        Ok(await _service.GetOwnerPurchaseOffersAsync(DemoIdentity.OwnerId));

    [HttpGet("{id:int}")]
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
    public Task<ActionResult<PurchaseOfferResponseDto>> Accept(int id) =>
        Handle(async () => await _service.AcceptPurchaseOfferAsync(id, DemoIdentity.OwnerId));

    [HttpPost("{id:int}/reject")]
    public Task<ActionResult<PurchaseOfferResponseDto>> Reject(int id) =>
        Handle(async () => await _service.RejectPurchaseOfferAsync(id, DemoIdentity.OwnerId));

    [HttpPost("{id:int}/negotiation/counter")]
    public Task<ActionResult<PurchaseNegotiationOfferResponseDto>> Counter(int id, [FromBody] PurchaseCounterOfferDto dto) =>
        Handle(async () => await _service.CreatePurchaseCounterOfferAsync(id, UserId(), Role(), dto));

    [HttpPost("{id:int}/negotiation/offers/{offerId:int}/accept")]
    public Task<ActionResult<PurchaseOfferResponseDto>> AcceptCounter(int id, int offerId) =>
        Handle(async () => await _service.AcceptPurchaseNegotiationOfferAsync(id, offerId, UserId(), Role()));

    [HttpGet("{id:int}/negotiation/offers")]
    public async Task<ActionResult<IEnumerable<PurchaseNegotiationOfferResponseDto>>> Offers(int id)
    {
        try { return Ok(await _service.GetPurchaseNegotiationOffersAsync(id, UserId(), Role())); }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { return Error(ex); }
    }

    [HttpPost("{id:int}/negotiation/messages")]
    public Task<ActionResult<NegotiationMessageResponseDto>> SendMessage(int id, [FromBody] NegotiationMessageDto dto) =>
        Handle(async () => await _service.AddPurchaseMessageAsync(id, UserId(), Role(), dto));

    [HttpGet("{id:int}/negotiation/messages")]
    public async Task<ActionResult<IEnumerable<NegotiationMessageResponseDto>>> Messages(int id)
    {
        try { return Ok(await _service.GetPurchaseMessagesAsync(id, UserId(), Role())); }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { return Error(ex); }
    }

    [HttpGet("{id:int}/agreement")]
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
    public Task<ActionResult<PurchaseAgreementResponseDto>> ConfirmAgreement(int id) =>
        Handle(async () => await _service.ConfirmPurchaseAgreementAsync(id, UserId(), Role()));

    private int UserId() => DemoIdentity.GetUserId(Request);
    private string Role() => DemoIdentity.GetRole(Request).ToString();

    private async Task<ActionResult<T>> Handle<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { return Error(ex); }
    }

    private ActionResult Error(Exception ex) => ex is UnauthorizedAccessException ? ForbidWithMessage(ex.Message) : BadRequest(new { message = ex.Message });
    private ObjectResult ForbidWithMessage(string message) => StatusCode(StatusCodes.Status403Forbidden, new { message });
}
