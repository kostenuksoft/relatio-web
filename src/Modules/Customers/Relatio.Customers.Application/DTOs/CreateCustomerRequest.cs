namespace Relatio.Customers.Application.DTOs;

public sealed record CreateCustomerRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Industry { get; init; }
}
