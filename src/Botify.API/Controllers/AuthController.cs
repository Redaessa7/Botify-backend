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
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Errors);
    }

    /// <summary>
    /// Login user
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authServices.LoginAsync(request);
        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(result.Errors);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await authServices.RefreshTokenAsync(request);
        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(result.Errors);
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        var result = await authServices.LogoutAsync(request);
        return result.IsSuccess
            ? Ok()
            : BadRequest(result.Errors);
    }
}
