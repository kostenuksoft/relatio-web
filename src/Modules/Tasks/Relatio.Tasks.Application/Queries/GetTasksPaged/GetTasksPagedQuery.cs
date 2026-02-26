using ErrorOr;
using MediatR;
using Relatio.Tasks.Application.DTOs;
using Relatio.Shared.Models;

namespace Relatio.Tasks.Application.Queries.GetTasksPaged;

public sealed record GetTasksPagedQuery(
    string? Status = null,
    string? Priority = null,
    Guid? AssignedToUserId = null,
    Guid? Cursor = null,
    int PageSize = 20) : IRequest<ErrorOr<CursorResult<TaskDto>>>;
