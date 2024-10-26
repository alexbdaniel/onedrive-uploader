using System.Threading.Channels;
using Microsoft.Extensions.Hosting;

namespace Application;

public class WatcherBackgroundService : BackgroundService
{
    private readonly Watcher watcher;

    public WatcherBackgroundService(Watcher watcher)
    {
        this.watcher = watcher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await watcher.Watch(stoppingToken);

       
        
    }
}