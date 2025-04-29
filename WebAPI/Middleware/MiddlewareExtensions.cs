
using WebAPI.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseValidationExceptionHandling(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<ValidationExceptionMiddleware>();
        
        return builder;
    }
}