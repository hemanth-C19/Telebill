using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Telebill.Dto.IdentityAccess;
using Telebill.Models;
using Telebill.Services.IdentityAccess;

namespace Telebill.Middlewares;

public class AuditLoggerMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            try
            {
                await TryWriteAuditAsync(context, auditService);
            }
            catch
            {
                Console.WriteLine("AuditLog failed");
            }
        }
    }

    private async Task TryWriteAuditAsync(HttpContext context, IAuditService auditService)
    {
        if (!ShouldAudit(context))
            return;

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return;

        var userEmail = context.User.FindFirstValue(ClaimTypes.Name);
        var userRole = context.User.FindFirstValue(ClaimTypes.Role);

        var path = context.Request.Path.Value ?? string.Empty;
        if (IsLoginPath(path))
            return;

        var method = context.Request.Method;
        var action = method;
        var metadata = JsonSerializer.Serialize(new
        {
            Email = userEmail,
            Role = userRole
        });

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

        return true;
    }

    private static bool IsLoginPath(string path)
    {
        return path.EndsWith("/login", StringComparison.OrdinalIgnoreCase);
    }
}
