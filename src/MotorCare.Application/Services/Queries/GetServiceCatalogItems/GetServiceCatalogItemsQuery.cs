using MediatR;
using MotorCare.Application.Common.Models;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Services.Queries.GetServiceCatalogItems;

public sealed record GetServiceCatalogItemsQuery(
    string? SearchText,
    ServiceCategory? Category,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<ServiceCatalogItemDto>>;
