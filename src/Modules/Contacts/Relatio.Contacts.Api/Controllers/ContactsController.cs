using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relatio.Contacts.Application.Commands.CreateContact;
using Relatio.Contacts.Application.Commands.DeleteContact;
using Relatio.Contacts.Application.Commands.RestoreContact;
using Relatio.Contacts.Application.Commands.UpdateContact;
using Relatio.Contacts.Application.DTOs;
using Relatio.Contacts.Application.Queries.GetContactById;
using Relatio.Contacts.Application.Queries.GetContactsPaged;
using Relatio.Shared.Controllers;

namespace Relatio.Contacts.Api.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contacts")]
public sealed class ContactsController : ApiController
{
    public ContactsController(ISender sender) : base(sender)
    {
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateContactRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateContactCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Position,
            request.CustomerId);

        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            contact => CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact),
            errors => HandleErrors(errors));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetContactByIdQuery(id);
        var result = await Sender.Send(query, cancellationToken);

        return result.Match(
            contact => Ok(contact),
            errors => HandleErrors(errors));
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] Guid? customerId,
        [FromQuery] string? name,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetContactsPagedQuery(customerId, name, sortBy, sortDirection, page, pageSize);
        var result = await Sender.Send(query, cancellationToken);

        return result.Match(
            pagedResult => Ok(pagedResult),
            errors => HandleErrors(errors));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateContactRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateContactCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Position);

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
        var command = new DeleteContactCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new RestoreContactCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }
}
