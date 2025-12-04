using Botify.Application.Responses;

namespace Botify.API.Extensions;


public static class SetTokenCookiesExtensions
{
    public static async Task SetTokenCookiesAsync(this HttpContext httpContext, TokenResponse tokens)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // true لو https
            SameSite = SameSiteMode.None,
            Expires = tokens.AccessTokenExpires
        };

        httpContext.Response.Cookies.Append("access_token", tokens.AccessToken!, cookieOptions);

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = tokens.RefreshTokenExpires
        };

        httpContext.Response.Cookies.Append("refresh_token", tokens.RefreshToken!, refreshCookieOptions);
    }

}