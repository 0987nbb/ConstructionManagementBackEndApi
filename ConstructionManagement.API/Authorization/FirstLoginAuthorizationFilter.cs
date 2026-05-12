using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConstructionManagement.API.Authorization;

/// <summary>
/// Blocks authenticated users who still have first-login onboarding until they call the allowed endpoints.
/// </summary>
public sealed class FirstLoginAuthorizationFilter : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowAnonymousAttribute))
            return Task.CompletedTask;

        var http = context.HttpContext;
        if (http.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        var path = http.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (path.StartsWith("/openapi", StringComparison.Ordinal) ||
            path.StartsWith("/scalar", StringComparison.Ordinal))
            return Task.CompletedTask;

        var isFirst = string.Equals(
            http.User.FindFirst(JwtCustomClaims.IsFirstLogin)?.Value,
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (!isFirst)
            return Task.CompletedTask;

        var method = http.Request.Method.ToUpperInvariant();
        if (IsAllowedForFirstLogin(method, path))
            return Task.CompletedTask;

        context.Result = new ObjectResult(ApiResponseDto<object>.Fail(
            "Complete first-login (password and profile) before accessing this resource."))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

        return Task.CompletedTask;
    }

    private static bool IsAllowedForFirstLogin(string method, string path)
    {
        if (method == "GET" && path.StartsWith("/api/users/profile", StringComparison.Ordinal))
            return true;
        if (method == "PUT" && path.StartsWith("/api/users/profile", StringComparison.Ordinal))
            return true;
        if (method == "GET" && path.Equals("/api/auth/me", StringComparison.Ordinal))
            return true;
        return method == "POST" && path.Equals("/api/auth/complete-first-login", StringComparison.Ordinal);
    }
}
