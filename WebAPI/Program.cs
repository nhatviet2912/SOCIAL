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

builder.Services.AddControllers();
builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", builder =>
    {
        builder.WithOrigins("http://localhost:4200") // Đổi thành URL frontend
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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
app.UseCors("Frontend");

app.ConfigureExceptionHandler(logger);
app.UseValidationExceptionHandling();
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.Run();