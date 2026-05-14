using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ConstructionManagement.API.Authorization;

public sealed class FirstLoginAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IUserRepository _userRepository;

    public FirstLoginAuthorizationFilter(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowAnonymousAttribute))
            return;

        var http = context.HttpContext;
        if (http.User.Identity?.IsAuthenticated != true)
            return;

        var path = http.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (path.StartsWith("/openapi", StringComparison.Ordinal) || path.StartsWith("/scalar", StringComparison.Ordinal))
            return;

        var userIdClaim = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new UnauthorizedObjectResult(ApiResponseDto<object>.Fail("Invalid token subject."));
            return;
        }

        var user = await _userRepository.GetByIdActiveAsync(userId);
        if (user == null || !user.IsActive)
        {
            context.Result = new UnauthorizedObjectResult(ApiResponseDto<object>.Fail("User is inactive or not found."));
            return;
        }

        if (!user.IsFirstLogin)
            return;

        var method = http.Request.Method.ToUpperInvariant();
        if (IsAllowedForFirstLogin(method, path))
            return;

        context.Result = new ObjectResult(ApiResponseDto<object>.Fail(
            "Complete first-login (password and profile) before accessing this resource."))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }

    private static bool IsAllowedForFirstLogin(string method, string path)
    {
        if (method == "GET" && path.StartsWith("/api/users/profile", StringComparison.Ordinal))
            return true;
        if (method == "PUT" && path.StartsWith("/api/users/profile", StringComparison.Ordinal))
            return true;
        if (method == "GET" && path.Equals("/api/auth/me", StringComparison.Ordinal))
            return true;
        if (method == "POST" && path.Equals("/api/auth/complete-first-login", StringComparison.Ordinal))
            return true;
        if (method == "POST" && path.Equals("/api/auth/logout", StringComparison.Ordinal))
            return true;
        return method == "POST" && path.Equals("/api/auth/refresh", StringComparison.Ordinal);
    }
}
