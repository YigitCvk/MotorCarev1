using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>;

public sealed record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    string RoleText,
    bool IsActive,
    bool IsEmailVerified,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt);
