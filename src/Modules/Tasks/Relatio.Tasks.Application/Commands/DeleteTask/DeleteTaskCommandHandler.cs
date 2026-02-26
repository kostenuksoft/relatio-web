using ErrorOr;
using MediatR;
using Relatio.Tasks.Domain.Errors;
using Relatio.Tasks.Domain.Interfaces;

namespace Relatio.Tasks.Application.Commands.DeleteTask;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, ErrorOr<Success>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITasksUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(
        ITaskRepository taskRepository,
        ITasksUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        DeleteTaskCommand command,
        CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (task is null)
            return TaskErrors.NotFound;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _taskRepository.DeleteAsync(task, cancellationToken);
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
