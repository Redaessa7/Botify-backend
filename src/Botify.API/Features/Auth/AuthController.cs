using Botify.API.CrossCutting.Controllers;
using Botify.API.CrossCutting.Extensions;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Requests;
using Botify.Application.Features.Auths.Requests;
using Botify.Application.Services.Auths;
using Botify.Domain.ResultPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Botify.API.Features.Auth;

[ApiVersion("1.0")]
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

    [HttpPost("check-old-password")]
    [Authorize]
    public async Task<IActionResult> CheckOldPassword([FromBody] CheckOldPasswordRequest request, CancellationToken ct)
    {
        var result = await authServices.CheckOldPasswordAsync(request, ct);
        return result.Match(_ => Ok(new {isOld = _}), Problem);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await authServices.ChangePasswordAsync(request, ct);
        return result.Match(_ => Ok(new {}), Problem);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var result = await authServices.ForgotPasswordAsync(request, ct);
        return result.Match(_ => Ok(new { }), Problem);
    }

    [HttpPost("generate-email-change-otp")]
    [Authorize]
    public async Task<IActionResult> GenerateEmailChangeOtp(GenerateEmailChangeOtpRequest request, CancellationToken ct)
    {
        var result = await authServices.GenerateEmailChangeOtpAsync(request, ct);
        return result.Match(_ => Ok(new { token = _ }), Problem);
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken ct)
    {
        var result = await authServices.VerifyOtpAsync(request, ct);
       
        return result.Match(_ => Ok(new {  token = _ }), Problem);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail(CancellationToken ct)
    {
        var email = Request.Headers["X-Email"].ToString();
        var resetToken = Request.Headers["X-Reset-Token"].ToString();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(resetToken))
            return Problem([Error.Unauthorized("ResetToken.Invalid", "Email or reset token is missing in headers")]);

        var request = new VerifyEmailRequest(email, resetToken);
        var result = await authServices.VerifyEmailAsync(request, ct);

        if (!result.IsSuccess)
            return Problem(result.Errors);

        await HttpContext.SetTokenCookiesAsync(result.Value);

        return Ok(new { message = "Email verified successfully" });
    }

    [HttpPost("reset-password")]
    [Authorize(Policy = "ResetTokenPolicy")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        // Email and ResetToken will come from headers, not body
        var email = Request.Headers["X-Email"].ToString();
        var resetToken = Request.Headers["X-Reset-Token"].ToString();
        if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(resetToken))
            return Problem([Error.Unauthorized("ResetToken.Invalid", "Email or reset token is missing in headers")]);
        // Override values in request with header values
        var updatedRequest = request with { Email = email, ResetToken = resetToken };

        var result = await authServices.ResetPasswordAsync(updatedRequest, ct);
        return result.Match(_ => Ok(new { message = "Password reset successfully" }), Problem);
    }

    [HttpPost("update-email")]
    [Authorize(Policy = "ResetTokenPolicy")]
    public async Task<IActionResult> UpdateEmailAsync(UpdateEmailRequest request,CancellationToken ct)
    {
        // Email and ResetToken will come from headers, not body
        var email = Request.Headers["X-Email"].ToString();
        var resetToken = Request.Headers["X-Reset-Token"].ToString();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(resetToken))
            return Problem([Error.Unauthorized("ResetToken.Invalid", "Email or reset token is missing in headers")]);

        var updatedRequest =  request with { NewEmail = email, ResetToken = resetToken };

        var result = await authServices.UpdateEmailAsync(updatedRequest, ct);
        return result.Match(_ => Ok(new { message = "Password reset successfully" }), Problem);
    }
}
