using ErrorOr;
using MediatR;
using Relatio.Tasks.Domain.Enums;
using Relatio.Tasks.Domain.Errors;
using Relatio.Tasks.Domain.Interfaces;

namespace Relatio.Tasks.Application.Commands.UpdateTask;

public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, ErrorOr<Success>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITasksUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(
        ITaskRepository taskRepository,
        ITasksUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateTaskCommand command,
        CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (task is null)
            return TaskErrors.NotFound;

        var priority = Enum.Parse<CrmTaskPriority>(command.Priority, true);
        var updateResult = task.Update(command.Title, command.Description, command.DueDate, priority, command.AssignedToUserId);
        if (updateResult.IsError)
            return updateResult.Errors;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _taskRepository.UpdateAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
