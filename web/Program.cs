using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;

namespace web
{
    public class Program
    {
        public static IConfiguration CurrentConfiguration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        /*public static int Main(string[] args)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Information))
                .MinimumLevel.Override("System", new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Information))
                //.WriteTo.LiterateConsole();
                .WriteTo.Console();

            var fileLogPath = Environment.GetEnvironmentVariable("APP_LOG_FILE_PATH");

            int fileLogFileCount = 0;
            int.TryParse(Environment.GetEnvironmentVariable("APP_LOG_FILE_COUNT"), out fileLogFileCount);

            if(!string.IsNullOrEmpty(fileLogPath))
                loggerConfig = loggerConfig.WriteTo.File(fileLogPath, rollingInterval: RollingInterval.Day, outputTemplate:"{Timestamp:o} [{Level:u4}] {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: (fileLogFileCount > 0 ? (int?)fileLogFileCount : null));

            Log.Logger = loggerConfig
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                var host = CreateHostBuilder(args).Build();
                
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }*/

        public static int Main(string[] args)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Information))
                .MinimumLevel.Override("System", new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Information))
                .WriteTo.Console();

            var fileLogPath = Environment.GetEnvironmentVariable("APP_LOG_FILE_PATH");
            int fileLogFileCount = int.TryParse(Environment.GetEnvironmentVariable("APP_LOG_FILE_COUNT"), out var count) ? count : 15; // Default to 15 if not set

            if (!string.IsNullOrEmpty(fileLogPath))
            {
                loggerConfig = loggerConfig.WriteTo.File(
                    fileLogPath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:o} [{Level:u4}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: fileLogFileCount > 0 ? (int?)fileLogFileCount : null
                );
            }

            Log.Logger = loggerConfig
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host with log file path: {FileLogPath} and file count limit: {FileLogFileCount}", fileLogPath, fileLogFileCount);
                var host = CreateHostBuilder(args).Build();

                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseSerilog();
                webBuilder.UseUrls("http://0.0.0.0:8080"); // Ensure the application listens on all network interfaces
            });
    }
}
