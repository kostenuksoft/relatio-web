using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Contacts.Application.DTOs;
using Relatio.Contacts.Domain.Entities;
using Relatio.Contacts.Domain.Errors;
using Relatio.Contacts.Domain.Interfaces;
using Relatio.Shared.ValueObjects;

namespace Relatio.Contacts.Application.Commands.CreateContact;

public sealed class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, ErrorOr<ContactDto>>
{
    private readonly IContactRepository _contactRepository;
    private readonly IContactsUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateContactCommandHandler(
        IContactRepository contactRepository,
        IContactsUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ErrorOr<ContactDto>> Handle(
        CreateContactCommand command,
        CancellationToken cancellationToken)
    {
        Email? email = null;
        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            email = Email.CreateUnsafe(command.Email.Trim().ToLowerInvariant());
            var emailExists = await _contactRepository.ExistsByEmailAsync(email, cancellationToken);
            if (emailExists)
                return ContactErrors.EmailAlreadyExists;
        }

        PhoneNumber? phone = !string.IsNullOrWhiteSpace(command.Phone)
            ? PhoneNumber.CreateUnsafe(command.Phone.Trim())
            : null;

        var contactResult = Contact.Create(
            command.FirstName,
            command.LastName,
            email,
            phone,
            command.Position,
            command.CustomerId);

        if (contactResult.IsError)
            return contactResult.Errors;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _contactRepository.AddAsync(contactResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return _mapper.Map<ContactDto>(contactResult.Value);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
