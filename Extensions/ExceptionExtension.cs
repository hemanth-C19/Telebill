using Telebill.Middlewares;

namespace Telebill.Extensions;

public static class ExceptionExtension{
    public static void UseGlobalExceptionMiddleware(this IApplicationBuilder app){
        app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}