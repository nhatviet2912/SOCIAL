using Application.Common.Interfaces.Service;
using WebAPI.Service;

namespace WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, CurrentUserService>();
        return services;
    }
}