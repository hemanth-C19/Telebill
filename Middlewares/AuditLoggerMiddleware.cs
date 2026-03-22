using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Telebill.Dto.IdentityAccess;
using Telebill.Services.IdentityAccess;

namespace Telebill.Middlewares;

/// <summary>
/// Writes an <see cref="Models.AuditLog"/> row for authenticated API requests using <c>ClaimTypes.NameIdentifier</c> from the JWT.
/// Login is audited in <see cref="Services.Auth.AuthService"/> because the user is not authenticated in the pipeline yet.
/// </summary>
public class AuditLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public AuditLoggerMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            try
            {
                await TryWriteAuditAsync(context);
            }
            catch
            {
                // Never fail the response because audit persistence failed.
            }
        }
    }

    private async Task TryWriteAuditAsync(HttpContext context)
    {
        if (!ShouldAudit(context))
            return;

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return;

        var path = context.Request.Path.Value ?? string.Empty;
        if (IsLoginPath(path))
            return;

        var method = context.Request.Method;
        var action = $"{method} {path}";
        var metadata = JsonSerializer.Serialize(new
        {
            statusCode = context.Response.StatusCode,
            method,
            path
        });

        using var scope = _scopeFactory.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

        await auditService.AddAsync(new AuditLogDTO
        {
            UserId = userId,
            Action = action,
            Resource = path,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata
        });
    }

    private static bool ShouldAudit(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
            return false;

        // Only API traffic (adjust if you serve non-API paths from the same host)
        return true;
    }

    private static bool IsLoginPath(string path)
    {
        // Login is recorded in AuthService; pipeline has no JWT identity on that request.
        return path.EndsWith("/login", StringComparison.OrdinalIgnoreCase);
    }
}
