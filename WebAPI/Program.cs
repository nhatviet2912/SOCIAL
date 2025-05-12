using Application;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddSwaggerExtension();

builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("RequiredAdminManager", policy => policy.RequireRole("Admin", "Manager"));
    option.AddPolicy("RequiredUser", policy => policy.RequireRole("User"));
});

builder.Services.AddControllers();
builder.Host.UseSerilog();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Áp dụng migration nếu cần
        context.Database.Migrate();

        // Gọi seed
        await ApplicationUserSeed.SeedAsync(services);
    }
    catch (Exception ex)
    {
        
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.ConfigureExceptionHandler(logger);
app.UseValidationExceptionHandling();
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.Run();