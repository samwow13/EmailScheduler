using EmailScheduler;
using NLog;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using EmailScheduler.Services;
using EmailScheduler.Models;

try
{
    var builder = Host.CreateApplicationBuilder(args);
    
    // Configure email settings
    builder.Services.Configure<EmailSettings>(
        builder.Configuration.GetSection("EmailSettings"));
    
    // Register services
    builder.Services.AddSingleton<IEmailService, EmailService>();
    builder.Services.AddHostedService<Worker>();

    // Add NLog to the dependency injection container
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Logging.AddNLog();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    // NLog: catch setup errors
    Console.WriteLine($"Error during startup: {ex}");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit
    LogManager.Shutdown();
}
