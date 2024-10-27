using Application.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Tests;

public static class TestServiceConfigurator
{
    public static IServiceCollection ConfigureTestServices(this IServiceCollection services, HostApplicationBuilder builder)
    {
        services.ConfigureServices(builder);

        // services.AddSingleton<GraphService>();
        // services.AddSingleton<UserHandler>();
        // services.AddSingleton<Authenticator>();
        // services.AddSingleton<Uploader>();
        
        return services;
    }
    
    // private static IServiceCollection ConfigureOptions(this IServiceCollection services, HostApplicationBuilder builder)
    // {
    //     services.AddOptions<ConfigurationOptions>().Bind(builder.Configuration.GetSection(ConfigurationOptions.Key))
    //         .ValidateDataAnnotations()
    //         .Validate(OptionsValidator.Validate)
    //         .ValidateOnStart();
    //     
    //     services.AddOptions<RequestOptions>().Bind(builder.Configuration.GetSection(RequestOptions.Key))
    //         .ValidateDataAnnotations()
    //         .Validate(OptionsValidator.Validate)
    //         .ValidateOnStart();
    //     
    //     return services;
    // }

}