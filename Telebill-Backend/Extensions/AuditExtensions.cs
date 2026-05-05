using Telebill.Middlewares;

namespace Telebill.Extensions;

public static class AuditExtensions
{
    public static IApplicationBuilder UseAuditLogger(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuditLoggerMiddleware>();
    }
}
