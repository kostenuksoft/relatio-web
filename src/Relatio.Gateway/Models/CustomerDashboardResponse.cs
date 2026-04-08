namespace Relatio.Gateway.Models;

public sealed class CustomerDashboardResponse
{
    public CustomerDto? Customer { get; init; }
    public PagedContactsDto? Contacts { get; init; }
    public PagedDealsDto? Deals { get; init; }
    public PagedTasksDto? Tasks { get; init; }
    public List<string> Unavailable { get; init; } = new();
}

public sealed class CustomerDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Industry { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class PagedContactsDto
{
    public List<ContactDto> Items { get; init; } = new();
}

public sealed class ContactDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Position { get; init; }
}

public sealed class PagedDealsDto
{
    public List<DealDto> Items { get; init; } = new();
}

public sealed class DealDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Stage { get; init; } = string.Empty;
}

public sealed class PagedTasksDto
{
    public List<TaskDto> Items { get; init; } = new();
}

public sealed class TaskDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? DueDate { get; init; }
}
