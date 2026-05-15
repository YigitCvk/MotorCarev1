using MediatR;
using MotorCare.Application.Common.Interfaces;

namespace MotorCare.Application.PublicRecords.Queries.GetPublicInspectionReport;

public sealed record GetPublicInspectionReportQuery(string Slug) : IRequest<PublicInspectionReportDto?>;

public sealed class GetPublicInspectionReportQueryHandler
    : IRequestHandler<GetPublicInspectionReportQuery, PublicInspectionReportDto?>
{
    private readonly IPublicRecordAccessService _publicRecordAccessService;

    public GetPublicInspectionReportQueryHandler(IPublicRecordAccessService publicRecordAccessService)
    {
        _publicRecordAccessService = publicRecordAccessService;
    }

    public Task<PublicInspectionReportDto?> Handle(
        GetPublicInspectionReportQuery request,
        CancellationToken cancellationToken)
    {
        return _publicRecordAccessService.GetInspectionReportAsync(request.Slug, cancellationToken);
    }
}
