using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Tests;

public class TestServiceProvider : IDisposable, IAsyncDisposable
{
    private ServiceProvider provider;
    private bool disposed = false;

    public ServiceProvider GetTestServiceProvider()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        string environmentName = builder.Environment.EnvironmentName.ToLower();
        
        string currentDirectoryName = Directory.GetCurrentDirectory();
        DirectoryInfo repositoryDirectory = new DirectoryInfo(currentDirectoryName).Parent!.Parent!.Parent!.Parent;
        string appsettingsPath = Path.Combine(repositoryDirectory!.FullName, "Application/appsettings.json");

        bool exists = File.Exists(appsettingsPath);
        
     
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(appsettingsPath, false)
            .AddJsonFile($"appsettings.test.json", true)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables();

        var services = builder.Services;

        services.ConfigureTestServices(builder);

        provider = services.BuildServiceProvider();

        return provider;
    }
    
 

    public void Dispose()
    {
        if (disposed) return;
        if (provider == null) return;
        
        provider.Dispose();
        disposed = true;
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (provider != null && !disposed) 
            await provider.DisposeAsync();

        disposed = true;
    }
}