using ErrorOr;
using MediatR;
using Relatio.Contacts.Domain.Errors;
using Relatio.Contacts.Domain.Interfaces;
using Relatio.Shared.ValueObjects;

namespace Relatio.Contacts.Application.Commands.UpdateContact;

public sealed class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, ErrorOr<Success>>
{
    private readonly IContactRepository _contactRepository;
    private readonly IContactsUnitOfWork _unitOfWork;

    public UpdateContactCommandHandler(
        IContactRepository contactRepository,
        IContactsUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateContactCommand command,
        CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (contact is null)
            return ContactErrors.NotFound;

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            email = Email.CreateUnsafe(command.Email.Trim().ToLowerInvariant());
            var emailExists = await _contactRepository.ExistsByEmailExcludingAsync(email, command.Id, cancellationToken);
            if (emailExists)
                return ContactErrors.EmailAlreadyExists;
        }

        PhoneNumber? phone = !string.IsNullOrWhiteSpace(command.Phone)
            ? PhoneNumber.CreateUnsafe(command.Phone.Trim())
            : null;

        var updateResult = contact.Update(command.FirstName, command.LastName, email, phone, command.Position);
        if (updateResult.IsError)
            return updateResult.Errors;

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
