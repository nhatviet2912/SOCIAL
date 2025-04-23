using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Middleware;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Copy original response stream
        var originalBody = context.Response.Body;
        using var newBody = new MemoryStream();
        context.Response.Body = newBody;

        await _next(context);

        if (context.Response.StatusCode == StatusCodes.Status400BadRequest &&
            context.Request.HasFormContentType == false &&
            context.Items.ContainsKey("errors") == false)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();

            var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(bodyText);
            if (problemDetails?.Errors != null)
            {
                var formatted = new
                {
                    errors = problemDetails.Errors.ToDictionary(
                        e => e.Key,
                        e => e.Value
                    )
                };

                context.Response.Body = originalBody;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(formatted));
                return;
            }
        }

        // Default response
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        await newBody.CopyToAsync(originalBody);
    }
}