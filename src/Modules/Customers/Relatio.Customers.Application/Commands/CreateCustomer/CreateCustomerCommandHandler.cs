using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Customers.Application.DTOs;
using Relatio.Customers.Domain.Entities;
using Relatio.Customers.Domain.Errors;
using Relatio.Customers.Domain.Interfaces;
using Relatio.Shared.ValueObjects;
namespace Relatio.Customers.Application.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, ErrorOr<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomersUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ICustomersUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

            return _mapper.Map<CustomerDto>(customerResult.Value);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
