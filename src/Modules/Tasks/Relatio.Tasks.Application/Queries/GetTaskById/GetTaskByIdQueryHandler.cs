using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Tasks.Application.DTOs;
using Relatio.Tasks.Domain.Errors;
using Relatio.Tasks.Domain.Interfaces;

namespace Relatio.Tasks.Application.Queries.GetTaskById;

public sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, ErrorOr<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;

    public GetTaskByIdQueryHandler(
        ITaskRepository taskRepository,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<TaskDto>> Handle(
        GetTaskByIdQuery query,
        CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(query.Id, cancellationToken);
        if (task is null)
            return TaskErrors.NotFound;

        return _mapper.Map<TaskDto>(task);
    }
}
