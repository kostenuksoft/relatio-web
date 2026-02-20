using Microsoft.AspNetCore.Mvc;
using MediatR;
using Asp.Versioning;
using Relatio.Customers.Application.Commands.CreateCustomer;
using Relatio.Customers.Application.Commands.UpdateCustomer;
using Relatio.Customers.Application.Commands.DeleteCustomer;
using Relatio.Customers.Application.Commands.ActivateCustomer;
using Relatio.Customers.Application.Commands.DeactivateCustomer;
using Relatio.Customers.Application.Queries.GetCustomerById;
using Relatio.Customers.Application.Queries.GetCustomersPaged;
using Relatio.Customers.Application.DTOs;
using Relatio.Shared.Controllers;

namespace Relatio.Customers.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customers")]
public sealed class CustomersController : ApiController
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.Name,
            request.Email,
            request.Phone,
            request.Industry);

        var result = await _sender.Send(command, cancellationToken);

        return result.Match(
            customer => CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer),
            errors => HandleErrors(errors));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return result.Match(
            customer => Ok(customer),
            errors => HandleErrors(errors));
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? name,
        [FromQuery] string? status,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomersPagedQuery(name, status, sortBy, sortDirection, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        return result.Match(
            pagedResult => Ok(pagedResult),
            errors => HandleErrors(errors));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.Industry);

        var result = await _sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCustomerCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ActivateCustomerCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateCustomerCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }
}
