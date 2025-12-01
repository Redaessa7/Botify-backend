using System.Security.Claims;
using Botify.Application.Common.Interfaces;

public class CurrentUser : IUser
{
    private readonly ClaimsPrincipal? _user;

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
