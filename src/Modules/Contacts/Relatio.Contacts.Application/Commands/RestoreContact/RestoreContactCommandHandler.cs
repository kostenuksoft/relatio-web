using ErrorOr;
using MediatR;
using Relatio.Contacts.Domain.Errors;
using Relatio.Contacts.Domain.Interfaces;

namespace Relatio.Contacts.Application.Commands.RestoreContact;

public sealed class RestoreContactCommandHandler : IRequestHandler<RestoreContactCommand, ErrorOr<Success>>
{
    private readonly IContactRepository _contactRepository;
    private readonly IContactsUnitOfWork _unitOfWork;

    public RestoreContactCommandHandler(
        IContactRepository contactRepository,
        IContactsUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        RestoreContactCommand command,
        CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetDeletedByIdTrackedAsync(command.Id, cancellationToken);
        if (contact is null)
            return ContactErrors.NotFound;

        var restoreResult = contact.Restore();
        if (restoreResult.IsError)
            return restoreResult.Errors;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _contactRepository.UpdateAsync(contact, cancellationToken);
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
