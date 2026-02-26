using ErrorOr;
using MediatR;
using Relatio.Customers.Domain.Errors;
using Relatio.Customers.Domain.Interfaces;

namespace Relatio.Customers.Application.Commands.DeleteCustomer;

public sealed class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, ErrorOr<Success>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomersUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ICustomersUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        DeleteCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (customer is null)
            return CustomerErrors.NotFound;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _customerRepository.DeleteAsync(customer, cancellationToken);
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
