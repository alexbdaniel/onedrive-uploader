using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Application.Graph;
using Application.UserInteractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Application.Configuration;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
public static class ServiceConfigurator
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, HostApplicationBuilder builder)
    {

        services.ConfigureOptions(builder);
        services.ConfigureGraphHttpClient();
        
        services.AddSingleton<FileQueue>();
        
        services.AddSingleton<GraphService>();
        services.AddScoped<UserHandler>();
        services.AddSingleton<Authenticator>();
        services.AddSingleton<Uploader>();
        
        services.AddMemoryCache();
        services.AddSingleton<Watcher>();
        services.AddHostedService<WatcherBackgroundService>();
        services.AddHostedService<QueueBackgroundService>();

        
        return services;
    }
    
    private static IServiceCollection ConfigureOptions(this IServiceCollection services, HostApplicationBuilder builder)
    {
        services.AddOptions<ConfigurationOptions>().Bind(builder.Configuration.GetSection(ConfigurationOptions.Key))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<UploadOptions>().Bind(builder.Configuration.GetSection(UploadOptions.Key))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<GraphOptions>().Bind(builder.Configuration.GetSection(GraphOptions.Key))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        
        
        
        return services;
    }
    
    private static IServiceCollection ConfigureGraphHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<GraphService>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<GraphOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseAddress);
            
            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options);
            
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
        });
        
 
        
        
        return services;
    }
    
}