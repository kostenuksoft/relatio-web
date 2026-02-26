using ErrorOr;
using MediatR;
using Relatio.Contacts.Application.DTOs;
using Relatio.Shared.Models;

namespace Relatio.Contacts.Application.Queries.GetContactsPaged;

public sealed record GetContactsPagedQuery(
    Guid? CustomerId = null,
    string? Name = null,
    string SortBy = "name",
    string SortDirection = "asc",
    int Page = 1,
    int PageSize = 20) : IRequest<ErrorOr<PagedResult<ContactDto>>>;
