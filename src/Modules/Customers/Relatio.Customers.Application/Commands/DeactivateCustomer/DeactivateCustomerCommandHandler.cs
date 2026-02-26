using ErrorOr;
using MediatR;
using Relatio.Customers.Domain.Errors;
using Relatio.Customers.Domain.Interfaces;

namespace Relatio.Customers.Application.Commands.DeactivateCustomer;

public sealed class DeactivateCustomerCommandHandler : IRequestHandler<DeactivateCustomerCommand, ErrorOr<Success>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomersUnitOfWork _unitOfWork;

    public DeactivateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ICustomersUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        DeactivateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (customer is null)
            return CustomerErrors.NotFound;

        var result = customer.Deactivate();
        if (result.IsError)
            return result.Errors;

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
