namespace Cash_Flow_Projection
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Core;

    public class Program
    {
        public static LoggingLevelSwitch LogLevel { get; } = new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Information);

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((context, builder) =>
                {
                    Log.Logger =
                           new LoggerConfiguration()
                           .Enrich.FromLogContext()
                           .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                           .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                           .Enrich.WithProperty("Version", $"{typeof(Startup).Assembly.GetName().Version}")
                           .WriteTo.Seq(serverUrl: "https://logs.redleg.app", apiKey: context.Configuration.GetValue<string>("Seq:ApiKey"), compact: true, controlLevelSwitch: LogLevel)
                           .MinimumLevel.ControlledBy(LogLevel)
                           .CreateLogger();

                    builder.AddSerilog();
                });
    }
}
