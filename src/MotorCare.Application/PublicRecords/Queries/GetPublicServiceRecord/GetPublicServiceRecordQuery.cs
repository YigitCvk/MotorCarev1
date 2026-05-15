using MediatR;
using MotorCare.Application.Common.Interfaces;

namespace MotorCare.Application.PublicRecords.Queries.GetPublicServiceRecord;

public sealed record GetPublicServiceRecordQuery(string Slug) : IRequest<PublicServiceRecordDto?>;

public sealed class GetPublicServiceRecordQueryHandler
    : IRequestHandler<GetPublicServiceRecordQuery, PublicServiceRecordDto?>
{
    private readonly IPublicRecordAccessService _publicRecordAccessService;

    public GetPublicServiceRecordQueryHandler(IPublicRecordAccessService publicRecordAccessService)
    {
        _publicRecordAccessService = publicRecordAccessService;
    }

    public Task<PublicServiceRecordDto?> Handle(
        GetPublicServiceRecordQuery request,
        CancellationToken cancellationToken)
    {
        return _publicRecordAccessService.GetServiceRecordAsync(request.Slug, cancellationToken);
    }
}
