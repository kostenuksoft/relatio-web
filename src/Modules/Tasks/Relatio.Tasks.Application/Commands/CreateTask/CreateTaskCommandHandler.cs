using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Tasks.Application.DTOs;
using Relatio.Tasks.Domain.Entities;
using Relatio.Tasks.Domain.Enums;
using Relatio.Tasks.Domain.Interfaces;

namespace Relatio.Tasks.Application.Commands.CreateTask;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, ErrorOr<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITasksUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        ITasksUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ErrorOr<TaskDto>> Handle(
        CreateTaskCommand command,
        CancellationToken cancellationToken)
    {
        var priority = Enum.Parse<CrmTaskPriority>(command.Priority, true);

        var taskResult = CrmTask.Create(
            command.Title,
            command.Description,
            command.DueDate,
            priority,
            command.AssignedToUserId);

        if (taskResult.IsError)
            return taskResult.Errors;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _taskRepository.AddAsync(taskResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return _mapper.Map<TaskDto>(taskResult.Value);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
