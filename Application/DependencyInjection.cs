﻿using System.Reflection;
using Application.Common.Interfaces.Service;
using Application.Service;
using Application.Validators.AuthValidator;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IExternalLoginService, ExternalLoginService>();
        services.AddScoped<IJWTHandler, JWTHandler>();


        services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
        services.AddFluentValidationAutoValidation();
        services.AddAutoMapper();
        
        return services;
    }
    
    private static void AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
    }
}