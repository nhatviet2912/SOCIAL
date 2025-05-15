using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Service;
using Domain.Constants;
using Domain.Entities;
using Domain.Entities.Settings;
using Infrastructure.Cache;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Infrastructure.Logs.Logging;
using Infrastructure.Repository;
using Infrastructure.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using StackExchange.Redis;
using Role = Domain.Constants.Role;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string"
                                                   + "'DefaultConnection' not found.");
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string");

        var version = ServerVersion.AutoDetect(connectionString);
        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseMySql(connectionString, version).UseLazyLoadingProxies());

        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        
        services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(redisConnectionString));
        
        services.Configure<IdentityOptions>(options =>
        {
            // Default Password settings.
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;
            
            options.SignIn.RequireConfirmedEmail = true;

            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ đĐáàảãạăắằẳẵặâấầẩẫậéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵ";
        });
        
        services.AddAuthorization(option =>
        {
            option.AddPolicy(Policies.AdminManager, policy => policy.RequireRole(Role.Admin, Role.Manager));
            option.AddPolicy(Policies.User, policy => policy.RequireRole(Role.User));
        });

        Log.Logger = SerilogLogger.Configure(configuration);
    
        services.AddLogging(loggingBuilder => 
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        });
        
        services.AddJwtExtension(configuration);

        services.Configure<EmailSettings>(configuration.GetSection("MailSettings"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<EmailSettings>>().Value);
        
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IDateTimeService, DateTimeService>();
        services.AddScoped<ICacheService, CacheRepository>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        return services;
    }
}