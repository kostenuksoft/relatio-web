using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Tasks.Application.DTOs;
using Relatio.Tasks.Domain.Enums;
using Relatio.Tasks.Domain.Interfaces;
using Relatio.Shared.Models;

namespace Relatio.Tasks.Application.Queries.GetTasksPaged;

public sealed class GetTasksPagedQueryHandler : IRequestHandler<GetTasksPagedQuery, ErrorOr<CursorResult<TaskDto>>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;

    public GetTasksPagedQueryHandler(
        ITaskRepository taskRepository,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<CursorResult<TaskDto>>> Handle(
        GetTasksPagedQuery query,
        CancellationToken cancellationToken)
    {
        CrmTaskStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
            status = Enum.Parse<CrmTaskStatus>(query.Status, true);

        CrmTaskPriority? priority = null;
        if (!string.IsNullOrWhiteSpace(query.Priority))
            priority = Enum.Parse<CrmTaskPriority>(query.Priority, true);

        var cursorResult = await _taskRepository.GetPagedAsync(
            status,
            priority,
            query.AssignedToUserId,
            query.Cursor,
            query.PageSize,
            cancellationToken);

        var items = _mapper.Map<List<TaskDto>>(cursorResult.Items);

        return new CursorResult<TaskDto>
        {
            Items = items,
            NextCursor = cursorResult.NextCursor
        };
    }
}
