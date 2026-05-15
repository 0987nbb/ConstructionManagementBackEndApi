using ConstructionManagement.BLL.Services;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConstructionManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await _auth.Register(dto);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await _auth.Login(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
        if (!result.Success) return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await _auth.RefreshTokenAsync(dto.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString());
        if (!result.Success) return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (!TryGetUserId(out var userId)) return Unauthorized(ApiResponseDto<object>.Fail("Invalid token subject."));

        var response = await _auth.LogoutAsync(userId, dto.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString());
        return response.Success ? Ok(response) : Unauthorized(response);
    }

    [HttpPost("set-password")]
    [AllowAnonymous]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await _auth.SetPassword(dto);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("validate-setup-token")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateSetupToken([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(ApiResponseDto<bool>.Fail("Token is required."));
        }

        var response = await _auth.ValidatePasswordSetupTokenAsync(token);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("request-password-reset")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _auth.RequestPasswordResetAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
        return response.Success ? Ok(response) : StatusCode(StatusCodes.Status500InternalServerError, response);
    }

    [HttpGet("validate-reset-token")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateResetToken([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(ApiResponseDto<bool>.Fail("Token is required."));
        }

        var response = await _auth.ValidateResetPasswordTokenAsync(token);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _auth.ResetPasswordAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("complete-first-login")]
    [Authorize]
    public async Task<IActionResult> CompleteFirstLogin([FromBody] CompleteFirstLoginDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token subject."));
        }

        var result = await _auth.CompleteFirstLogin(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Email),
            role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "Admin access granted." });
    }

    private bool TryGetUserId(out Guid userId)
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idValue, out userId);
    }
}
