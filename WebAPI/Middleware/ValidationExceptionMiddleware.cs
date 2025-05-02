using System.Text.Json;
using Domain.Constants;
using Domain.Exception;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Middleware;

public class FluentValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FluentValidationMiddleware> _logger;

    public FluentValidationMiddleware(
        RequestDelegate next,
        ILogger<FluentValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            
            var response = new Error(ErrorMessageResponse.VALIDATION_ERROR, context.Response.StatusCode, null, errors, new Guid());

            _logger.LogWarning(ErrorMessageResponse.VALIDATION_FAILED, errors);
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}