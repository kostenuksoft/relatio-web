using ErrorOr;

namespace Relatio.Sales.Application.Interfaces;

public interface ICustomerServiceClient
{
    Task<ErrorOr<bool>> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken = default);
}
