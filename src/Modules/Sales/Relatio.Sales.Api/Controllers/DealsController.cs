using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relatio.Sales.Application.Commands.ChangeDealStage;
using Relatio.Sales.Application.Commands.CreateDeal;
using Relatio.Sales.Application.Commands.DeleteDeal;
using Relatio.Sales.Application.Commands.UpdateDeal;
using Relatio.Sales.Application.DTOs;
using Relatio.Sales.Application.Queries.GetDealById;
using Relatio.Sales.Application.Queries.GetDealsPaged;
using Relatio.Shared.Controllers;

namespace Relatio.Sales.Api.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/deals")]
public sealed class DealsController : ApiController
{
    public DealsController(ISender sender) : base(sender)
    {
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDealRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDealCommand(
            request.Title,
            request.Amount,
            request.Currency,
            request.CustomerId,
            request.ExpectedCloseDate,
            request.Notes);

        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            deal => CreatedAtAction(nameof(GetById), new { id = deal.Id }, deal),
            errors => HandleErrors(errors));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetDealByIdQuery(id);
        var result = await Sender.Send(query, cancellationToken);

        return result.Match(
            deal => Ok(deal),
            errors => HandleErrors(errors));
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] Guid? customerId,
        [FromQuery] string? stage,
        [FromQuery] Guid? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDealsPagedQuery(customerId, stage, cursor, pageSize);
        var result = await Sender.Send(query, cancellationToken);

        return result.Match(
            pagedResult => Ok(pagedResult),
            errors => HandleErrors(errors));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDealRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDealCommand(
            id,
            request.Title,
            request.Amount,
            request.Currency,
            request.ExpectedCloseDate,
            request.Notes);

        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDealCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }

    [HttpPost("{id:guid}/stage")]
    public async Task<IActionResult> ChangeStage(
        Guid id,
        [FromBody] ChangeDealStageRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeDealStageCommand(id, request.Stage);
        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }
}
