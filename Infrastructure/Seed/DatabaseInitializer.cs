using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Seed;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Apply migrations
            await context.Database.MigrateAsync();

            // Seed initial data
            await ApplicationUserSeed.SeedAsync(services);
        }
        catch (Exception ex)
        {
            
        }
    }
}