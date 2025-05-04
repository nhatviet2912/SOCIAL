using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Infrastructure.Logs.Logging;

public static class SerilogLogger
{
    public static ILogger Configure(IConfiguration config)
    {
        var logPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            config["Logging:File:Directory"] ?? "Logs",
            "log_.txt");

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                formatter: new JsonFormatter())
            .CreateLogger();
    }
}