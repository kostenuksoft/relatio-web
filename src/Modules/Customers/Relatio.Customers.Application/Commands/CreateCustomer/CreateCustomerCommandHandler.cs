using ErrorOr;
using Mapster;
using MediatR;
using Relatio.Customers.Application.DTOs;
using Relatio.Customers.Domain.Entities;
using Relatio.Customers.Domain.Errors;
using Relatio.Customers.Domain.Interfaces;
using Relatio.Customers.Domain.ValueObjects;
using Relatio.Shared.Abstractions;

namespace Relatio.Customers.Application.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, ErrorOr<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<CustomerDto>> Handle(
        CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var email = Email.CreateUnsafe(command.Email.Trim().ToLowerInvariant());

        var emailExists = await _customerRepository.ExistsByEmailAsync(email, cancellationToken);
        if (emailExists)
            return CustomerErrors.EmailAlreadyExists;

        PhoneNumber? phone = !string.IsNullOrWhiteSpace(command.Phone)
            ? PhoneNumber.CreateUnsafe(command.Phone.Trim())
            : null;

        var customerResult = Customer.Create(command.Name, email, phone, command.Industry);
        if (customerResult.IsError)
            return customerResult.Errors;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _customerRepository.AddAsync(customerResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return customerResult.Value.Adapt<CustomerDto>();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
