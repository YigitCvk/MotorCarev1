using MediatR;

namespace MotorCare.Application.Auth.Queries.GetSecurityStatus;

public sealed record GetSecurityStatusQuery : IRequest<SecurityStatusDto>;
