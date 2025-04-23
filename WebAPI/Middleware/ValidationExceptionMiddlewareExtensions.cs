
using WebAPI.Middleware;

public static class ValidationExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseValidationExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ValidationExceptionMiddleware>();
    }
}