using Botify.API.Extensions;
using Botify.Application.Requests;
using Botify.Application.Services.Auths;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthServices authServices) : ControllerBase
{
    /// <summary>
    /// Register new user
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await authServices.RegisterAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        await HttpContext.SetTokenCookiesAsync(result.Value);
        return Ok(new { message = "register merchant is successfully" });
    }

    /// <summary>
    /// Login user
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authServices.LoginAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        await HttpContext.SetTokenCookiesAsync(result.Value);
        return Ok(new { message = "Logged in successfully" });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
               return Problem(
                title: "Unauthorized",
                detail: "Refresh token is missing",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        var result = await authServices.RefreshTokenAsync(refreshToken);

        if (!result.IsSuccess)
        {
             return Problem(
                title: "Unauthorized",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        var tokens = result.Value;
        await HttpContext.SetTokenCookiesAsync(tokens);

        return Ok(new { message = "Token refreshed successfully" });
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Optionally, revoke refresh token in DB if you want full logout server-side
        // await authServices.LogoutAsync(UserIdFromClaims);
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
            return Unauthorized(new { message = "Refresh token is missing" });
        }

        var result = await authServices.LogoutAsync(refreshToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(result.Errors);
        }

        // Delete auth cookies
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("user")]
    [Authorize]
    public IActionResult GetUserInfo()
    {
        var userInfo = User.UserInfo();

        if (userInfo.Id == null || userInfo.Email == null)
            return Unauthorized();

        return Ok(userInfo);
    }

}
