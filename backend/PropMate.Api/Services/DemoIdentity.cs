using Microsoft.AspNetCore.Http;
using PropMate.Api.Enums;

namespace PropMate.Api.Services;

public static class DemoIdentity
{
    public const int TenantId = 1;
    public const int OwnerId = 2;
    public const int AdminId = 3;
    public const int PropertyManagerId = 4;

    public static UserRole GetRole(HttpRequest request)
    {
        var requestedRole = request.Headers["X-Workspace-Role"].FirstOrDefault();
        return Enum.TryParse<UserRole>(requestedRole, ignoreCase: true, out var role) && Enum.IsDefined(role)
            ? role
            : UserRole.BuyerRenter;
    }

    public static int GetUserId(UserRole role) => role switch
    {
        UserRole.OwnerAgent => OwnerId,
        UserRole.Admin => AdminId,
        UserRole.PropertyManager => PropertyManagerId,
        _ => TenantId,
    };

    public static int GetUserId(HttpRequest request) => GetUserId(GetRole(request));
}
