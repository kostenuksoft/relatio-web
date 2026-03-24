namespace Relatio.Sales.Application.Interfaces;

public interface IServiceTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
}
