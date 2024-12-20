using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Application.Graph;
using Application.UserInteractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace Application.Configuration;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
public static class ServiceConfigurator
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, HostApplicationBuilder builder)
    {
        services.ConfigureOptions(builder);
        services.ConfigureLogging(builder);
        services.ConfigureGraphHttpClient();
        
        
        services.AddSingleton<GraphService>();
        services.AddScoped<UserHandler>();
        // services.AddSingleton<Authenticator>();
        services.AddSingleton<Uploader>();
        services.AddSingleton<FileQueue>();
        services.AddSingleton<Watcher>();
        
        services.AddMemoryCache();
        
        services.AddHostedService<WatcherBackgroundService>();
        services.AddHostedService<QueueBackgroundService>();
        
        return services;
    }
    
    private static IServiceCollection ConfigureLogging(this IServiceCollection services, HostApplicationBuilder builder)
    {
        using var provider = services.BuildServiceProvider();
        
        var logger = new LoggerConfiguration()
            .ConfigureSinks()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentUserName()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .CreateLogger();
        
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
        
        logger.Information("Logging configured.");
        
        return services;
    }
    
    [SuppressMessage("ReSharper", "InvertIf")]
    private static LoggerConfiguration ConfigureSinks(this LoggerConfiguration configuration, LogEventLevel defaultLevel = LogEventLevel.Information)
    {
        const string applicationName = "onedrive-uploader"; //AssemblyName.GetAssemblyName(AppContext.BaseDirectory).Name
        
        if (OperatingSystem.IsLinux())
        {
            string logDirectoryName = Path.Combine(Directory.GetCurrentDirectory(), "log");
            Directory.CreateDirectory(logDirectoryName);
            
            string logFullPath = Path.Combine(logDirectoryName, $"{applicationName}.log");
            
            configuration
                .WriteTo.LocalSyslog(appName: applicationName)
                .WriteTo.File(path: logFullPath, retainedFileCountLimit: 2)
                .WriteTo.Console();
            return configuration;
        }

        if (OperatingSystem.IsWindows())
        {
            configuration
                .WriteTo.Console(restrictedToMinimumLevel: defaultLevel);
            return configuration;
        }
        
        return configuration;
    }
    
    
    private static IServiceCollection ConfigureOptions(this IServiceCollection services, HostApplicationBuilder builder)
    {
        services.AddOptions<ConfigurationOptions>().Bind(builder.Configuration.GetSection(ConfigurationOptions.Key))
            .ValidateDataAnnotations()
            .Validate(OptionsValidator.Validate)
            .ValidateOnStart();
        
        services.AddOptions<UploadOptions>().Bind(builder.Configuration.GetSection(UploadOptions.Key))
            .ValidateDataAnnotations()
            .Validate(OptionsValidator.Validate)
            .ValidateOnStart();
        
        services.AddOptions<GraphOptions>().Bind(builder.Configuration.GetSection(GraphOptions.Key))
            .ValidateDataAnnotations()
            .Validate(OptionsValidator.Validate)
            .ValidateOnStart();
        
        return services;
    }
    
    private static IServiceCollection ConfigureGraphHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<GraphService>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<GraphOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseAddress);
            
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
        });
        
        return services;
    }
    
}