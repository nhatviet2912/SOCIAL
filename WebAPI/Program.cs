using Application;
using Infrastructure;
using Infrastructure.Seed;
using Serilog;
using WebAPI;
using WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebServices();
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

await DatabaseInitializer.InitializeAsync(app.Services);

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