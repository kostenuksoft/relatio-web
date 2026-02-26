using ErrorOr;
using MediatR;
using Relatio.Contacts.Domain.Errors;
using Relatio.Contacts.Domain.Interfaces;

namespace Relatio.Contacts.Application.Commands.DeleteContact;

public sealed class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, ErrorOr<Success>>
{
    private readonly IContactRepository _contactRepository;
    private readonly IContactsUnitOfWork _unitOfWork;

    public DeleteContactCommandHandler(
        IContactRepository contactRepository,
        IContactsUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        DeleteContactCommand command,
        CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (contact is null)
            return ContactErrors.NotFound;

        var deleteResult = contact.Delete();
        if (deleteResult.IsError)
            return deleteResult.Errors;

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
