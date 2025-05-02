using System;
using System.Net;
using Application.Common.Exception;
using Domain.Constants;
using Domain.Exception;
using Microsoft.AspNetCore.Diagnostics;

public static class ExceptionMiddleware
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            AllowStatusCode404Response = true,
            ExceptionHandler = async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var traceId = Guid.NewGuid();

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    string errorMessage = string.Empty;
                    int errorCode = 400;
                    string title = string.Empty;

                    if (contextFeature.Error is CustomException exception)
                    {
                        switch (exception.ErrorCode)
                        {
                            case StatusCodes.Status400BadRequest:
                                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                title = ResponseCode.BAD_REQUEST;
                                errorMessage = exception.Message;
                                errorCode = exception.ErrorCode;
                                break;
                            default:
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                title = ResponseCode.INTERNAL_ERROR;
                                errorMessage = exception.Message;
                                errorCode = (int)HttpStatusCode.InternalServerError;
                                break;
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        title = ResponseCode.INTERNAL_ERROR;
                        errorCode = (int)HttpStatusCode.InternalServerError;
                        errorMessage = ErrorMessageResponse.AN_ERROR;
                    }
                    await context.Response.WriteAsync(new Error(title, errorCode, errorMessage, null, traceId).ToString());
                    logger.LogError("ErrorId:{errorId} Exception:{contextFeature.Error}", traceId,
                        contextFeature.Error);
                }
            }
        });
    }
}