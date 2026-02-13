using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;

namespace Botify.API.CrossCutting.Filters;

    public class ResetTokenHandler : AuthorizationHandler<ResetTokenRequirement>
{
    private readonly IDistributedCache _redis;

    public ResetTokenHandler(IDistributedCache redis)
    {
        _redis = redis;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResetTokenRequirement requirement)
    {
        var httpContext = (context.Resource as Microsoft.AspNetCore.Http.DefaultHttpContext);
        var email = httpContext?.Request.Headers["X-Email"].ToString();
        var token = httpContext?.Request.Headers["X-Reset-Token"].ToString();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            return;

        var storedToken = await _redis.GetStringAsync($"reset:{email}");
        if (storedToken != null && storedToken == token)
        {
            context.Succeed(requirement);
        }
    }
}
