using MotorCare.App.Models.Users;

namespace MotorCare.App.Services;

public sealed class UsersService(ApiClient apiClient)
{
    public Task<IReadOnlyList<UserListItem>?> GetUsersAsync(CancellationToken ct = default)
        => apiClient.GetAsync<IReadOnlyList<UserListItem>>("/api/users", authorized: true, ct);

    public Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var payload = new
        {
            fullName = request.FullName,
            email = request.Email,
            password = request.Password,
            role = ToRoleValue(request.Role)
        };
        return apiClient.PostAsync<object, Guid>("/api/users", payload, authorized: true, ct)!;
    }

    public Task InviteUserAsync(string email, string role, string? fullName = null, CancellationToken ct = default)
    {
        var payload = new { email, role = ToRoleValue(role), fullName };
        return apiClient.PostAsync("/api/users/invite", payload, authorized: true, ct);
    }

    public Task UpdateRoleAsync(Guid id, string role, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/users/{id}/role", new { role = ToRoleValue(role) }, authorized: true, ct);

    public Task DeactivateAsync(Guid id, CancellationToken ct = default)
        => apiClient.PatchAsync($"/api/users/{id}/deactivate", new { }, authorized: true, ct);

    private static int ToRoleValue(string role) => role switch
    {
        "Owner" => 1,
        "Admin" => 2,
        "Receptionist" => 3,
        "Technician" => 4,
        "Manager" => 5,
        "Inspector" => 6,
        "Accountant" => 7,
        "ReadOnly" => 8,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Geçersiz kullanıcı rolü.")
    };
}
