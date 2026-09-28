using Microsoft.AspNetCore.Mvc;
using PropMate.Api.DTOs.Transactions;
using PropMate.Api.Enums;
using PropMate.Api.Services;
using PropMate.Api.Services.Interfaces;

namespace PropMate.Api.Controllers;

[ApiController]
[Route("api/rental-applications")]
public class RentalApplicationsController : ControllerBase
{
    private readonly ITransactionService _service;
    public RentalApplicationsController(ITransactionService service) => _service = service;

    [HttpPost]
    public Task<ActionResult<RentalApplicationResponseDto>> Create([FromBody] CreateRentalApplicationDto dto) =>
        Handle(async () => await _service.CreateRentalApplicationAsync(DemoIdentity.TenantId, dto));

    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<RentalApplicationResponseDto>>> Mine() =>
        Ok(await _service.GetMyRentalApplicationsAsync(DemoIdentity.TenantId));

    [HttpGet("owner")]
    public async Task<ActionResult<IEnumerable<RentalApplicationResponseDto>>> OwnerInbox() =>
        Ok(await _service.GetOwnerRentalApplicationsAsync(DemoIdentity.OwnerId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RentalApplicationResponseDto>> Get(int id)
    {
        try
        {
            var item = await _service.GetRentalApplicationAsync(id, UserId(), Role());
            return item == null ? NotFound() : Ok(item);
        }
        catch (UnauthorizedAccessException ex) { return ForbidWithMessage(ex.Message); }
    }

    [HttpPost("{id:int}/accept")]
    public Task<ActionResult<RentalApplicationResponseDto>> Accept(int id) =>
        Handle(async () => await _service.AcceptRentalApplicationAsync(id, DemoIdentity.OwnerId));

    [HttpPost("{id:int}/reject")]
    public Task<ActionResult<RentalApplicationResponseDto>> Reject(int id) =>
        Handle(async () => await _service.RejectRentalApplicationAsync(id, DemoIdentity.OwnerId));

    [HttpPost("{id:int}/negotiation/counter")]
    public Task<ActionResult<RentalNegotiationOfferResponseDto>> Counter(int id, [FromBody] RentalCounterOfferDto dto) =>
        Handle(async () => await _service.CreateRentalCounterOfferAsync(id, UserId(), Role(), dto));

    [HttpPost("{id:int}/negotiation/offers/{offerId:int}/accept")]
    public Task<ActionResult<RentalApplicationResponseDto>> AcceptCounter(int id, int offerId) =>
        Handle(async () => await _service.AcceptRentalNegotiationOfferAsync(id, offerId, UserId(), Role()));

    [HttpGet("{id:int}/negotiation/offers")]
    public async Task<ActionResult<IEnumerable<RentalNegotiationOfferResponseDto>>> Offers(int id)
    {
        try { return Ok(await _service.GetRentalNegotiationOffersAsync(id, UserId(), Role())); }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { return Error(ex); }
    }

    [HttpPost("{id:int}/negotiation/messages")]
    public Task<ActionResult<NegotiationMessageResponseDto>> SendMessage(int id, [FromBody] NegotiationMessageDto dto) =>
        Handle(async () => await _service.AddRentalMessageAsync(id, UserId(), Role(), dto));

    [HttpGet("{id:int}/negotiation/messages")]
    public async Task<ActionResult<IEnumerable<NegotiationMessageResponseDto>>> Messages(int id)
    {
        try { return Ok(await _service.GetRentalMessagesAsync(id, UserId(), Role())); }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { return Error(ex); }
    }

    [HttpGet("{id:int}/agreement")]
    public async Task<ActionResult<RentalAgreementResponseDto>> Agreement(int id)
    {
        try
        {
            var agreement = await _service.GetRentalAgreementAsync(id, UserId(), Role());
            return agreement == null ? NotFound() : Ok(agreement);
        }
        catch (UnauthorizedAccessException ex) { return ForbidWithMessage(ex.Message); }
    }

    [HttpPost("{id:int}/agreement/confirm")]
    public Task<ActionResult<RentalAgreementResponseDto>> ConfirmAgreement(int id) =>
        Handle(async () => await _service.ConfirmRentalAgreementAsync(id, UserId(), Role()));

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
