using System.Security.Authentication;
using Application.Configuration;
using Application.Graph;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application;

public class WatcherBackgroundService : BackgroundService
{
    private readonly Watcher watcher;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger logger;
    private readonly IHostApplicationLifetime lifetime;

    public WatcherBackgroundService(Watcher watcher, IServiceScopeFactory serviceScopeFactory, ILogger<WatcherBackgroundService> logger, IHostApplicationLifetime lifetime)
    {
        this.watcher = watcher;
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
        this.lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await watcher.Watch(stoppingToken);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await HandleStartupAsync();
        }
        catch (AuthenticationException)
        {
            Console.WriteLine("Unable to authenticate with Entra. Check that the application has been given appropriate permissions.");
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            await cancellationTokenSource.CancelAsync();

            lifetime.StopApplication();
            await StopAsync(cancellationToken);
            
            return;
        }
        
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        
        await base.StopAsync(cancellationToken);
    }

    private async Task HandleStartupAsync()
    {
        using var scope = serviceScopeFactory.CreateScope();
        
        IMemoryCache cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
        var configurationOptions = scope.ServiceProvider.GetRequiredService<IOptions<ConfigurationOptions>>().Value;
        var graphService = scope.ServiceProvider.GetRequiredService<GraphService>();
        
        var authenticator = new Authenticator(graphService, configurationOptions, cache, logger);
        await authenticator.UpdateRefreshAndAccessTokenAsync();
        
        await watcher.HandleInitialFiles();
    }
    

}