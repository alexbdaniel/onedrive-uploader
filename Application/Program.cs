using System.Reflection;
using Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Application;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (HandleUnhandledException);
        
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables();
            
        var services = builder.Services;
        
        services.ConfigureServices(builder);

        // var provider = services.BuildServiceProvider();

        IHost application = builder.Build();

        await application.RunAsync().ConfigureAwait(false);
    }
    
    private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            Exception ex = (Exception)e.ExceptionObject;
            Console.WriteLine($"An unhandled exception occured. {ex}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    
    
}