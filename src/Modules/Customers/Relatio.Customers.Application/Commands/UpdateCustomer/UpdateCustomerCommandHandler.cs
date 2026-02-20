using ErrorOr;
using MediatR;
using Relatio.Customers.Domain.Errors;
using Relatio.Customers.Domain.Interfaces;
using Relatio.Customers.Domain.ValueObjects;
using Relatio.Shared.Abstractions;

namespace Relatio.Customers.Application.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, ErrorOr<Success>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (customer is null)
            return CustomerErrors.NotFound;

        var email = Email.CreateUnsafe(command.Email.Trim().ToLowerInvariant());

        var emailExists = await _customerRepository.ExistsByEmailExcludingAsync(email, command.Id, cancellationToken);
        if (emailExists)
            return CustomerErrors.EmailAlreadyExists;

        PhoneNumber? phone = !string.IsNullOrWhiteSpace(command.Phone)
            ? PhoneNumber.CreateUnsafe(command.Phone.Trim())
            : null;

        var updateResult = customer.Update(command.Name, email, phone, command.Industry);
        if (updateResult.IsError)
            return updateResult.Errors;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _customerRepository.UpdateAsync(customer, cancellationToken);
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
