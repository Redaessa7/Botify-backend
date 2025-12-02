using System.Security.Claims;
using Botify.Application.Responses;

namespace Botify.API.Extensions;

public static class GetUserClaimsExtensions
{
    public static UserInfoResponse UserInfo(this ClaimsPrincipal user)
    {
        if (user == null) return null!;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        return new UserInfoResponse
        {
            Id = userId!,
            Email = email!,
            Roles = roles,
        };
    }
}
