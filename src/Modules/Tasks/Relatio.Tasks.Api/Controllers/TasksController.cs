using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relatio.Tasks.Application.Commands.CancelTask;
using Relatio.Tasks.Application.Commands.CompleteTask;
using Relatio.Tasks.Application.Commands.CreateTask;
using Relatio.Tasks.Application.Commands.DeleteTask;
using Relatio.Tasks.Application.Commands.UpdateTask;
using Relatio.Tasks.Application.DTOs;
using Relatio.Tasks.Application.Queries.GetTaskById;
using Relatio.Tasks.Application.Queries.GetTasksPaged;
using Relatio.Shared.Controllers;

namespace Relatio.Tasks.Api.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tasks")]
public sealed class TasksController : ApiController
{
    public TasksController(ISender sender) : base(sender)
    {
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand(
            request.Title,
            request.Description,
            request.DueDate,
            request.Priority,
            request.AssignedToUserId);

        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            task => CreatedAtAction(nameof(GetById), new { id = task.Id }, task),
            errors => HandleErrors(errors));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTaskByIdQuery(id);
        var result = await Sender.Send(query, cancellationToken);

        return result.Match(
            task => Ok(task),
            errors => HandleErrors(errors));
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] Guid? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTasksPagedQuery(status, priority, assignedToUserId, cursor, pageSize);
        var result = await Sender.Send(query, cancellationToken);

        return result.Match(
            pagedResult => Ok(pagedResult),
            errors => HandleErrors(errors));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(
            id,
            request.Title,
            request.Description,
            request.DueDate,
            request.Priority,
            request.AssignedToUserId);

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
        var command = new DeleteTaskCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CompleteTaskCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CancelTaskCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }
}
