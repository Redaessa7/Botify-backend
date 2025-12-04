using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Responses;
using System.Security.Claims;

public class CurrentUser : IUser
{
    private readonly ClaimsPrincipal? _user;
    public UserInfoResponse UserInfo()
    {
        var userId = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = _user?.FindFirst(ClaimTypes.Email)?.Value;
        var roles = _user?.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        return new UserInfoResponse
        {
            Id = userId!,
            Email = email!,
            Roles = roles ?? ["No Roles"],
        };
    }
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    public string? Id => _user?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Username => _user?.FindFirstValue("username");
    public string? Email => _user?.FindFirstValue(ClaimTypes.Email);
    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;

    public IEnumerable<string> Roles =>
        _user?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool HasClaim(string type, string value) =>
        _user?.HasClaim(type, value) == true;
}
