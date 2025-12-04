using Botify.API.CrossCutting.Controllers;
using Botify.API.CrossCutting.Extensions;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Requests;
using Botify.Application.Services.Auths;
using Botify.Domain.ResultPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Auth;

[Route("api/auth")]
public class AuthController(IAuthServices authServices, IUser user) : ApiController
{
    private readonly IUser _user = user;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await authServices.RegisterAsync(request);

        if (!result.IsSuccess)
            return Problem(result.Errors);

        await HttpContext.SetTokenCookiesAsync(result.Value);
        return Ok(new { message = "Register merchant successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authServices.LoginAsync(request);

        if (!result.IsSuccess)
            return Problem(result.Errors);

        await HttpContext.SetTokenCookiesAsync(result.Value);
        return Ok(new { message = "Logged in successfully" });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
            return Problem([Error.Unauthorized("RefreshToken.Missing", "Refresh token is missing")]);

        var result = await authServices.RefreshTokenAsync(refreshToken);

        if (!result.IsSuccess)
            return Problem(result.Errors);

        await HttpContext.SetTokenCookiesAsync(result.Value);
        return Ok(new { message = "Token refreshed successfully" });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
            return Problem([Error.Unauthorized("RefreshToken.Missing", "Refresh token is missing")]);

        var result = await authServices.LogoutAsync(refreshToken);

        if (!result.IsSuccess)
            return Problem(result.Errors);

        // ------ My code
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,                // if you set Secure = true originally
            SameSite = SameSiteMode.None, // if originally SameSite=None
            Path = "/",                   // should match original Path
            Domain="localhost"              // Domain = "example.com",    // include if you set Domain originally
        };

        // expire them explicitly (safer than Delete if you want to control attributes)
        Response.Cookies.Append("access_token", "", new CookieOptions(cookieOptions) { Expires = DateTimeOffset.UtcNow.AddDays(-1) });
        Response.Cookies.Append("refresh_token", "", new CookieOptions(cookieOptions) { Expires = DateTimeOffset.UtcNow.AddDays(-1) });

        //// or use Delete with the same options:
        //Response.Cookies.Delete("access_token", cookieOptions);
        //Response.Cookies.Delete("refresh_token", cookieOptions);

        //-----  your code
        //Response.Cookies.Delete("access_token");
        //Response.Cookies.Delete("refresh_token"); 

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("user")]
    [Authorize]
    public IActionResult GetUserInfo()
    {
        var userInfo = _user.UserInfo();

        if (string.IsNullOrEmpty(userInfo.Id) || string.IsNullOrEmpty(userInfo.Email))
            return Problem([Error.Unauthorized("User.Invalid", "User info is missing")]);

        return Ok(userInfo);
    }
}
