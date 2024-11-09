using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application;

public class QueueBackgroundService : BackgroundService
{
    private readonly FileQueue queue;
    private readonly ILogger logger;

    public QueueBackgroundService(FileQueue queue, ILogger<QueueBackgroundService> logger)
    {
        this.queue = queue;
        this.logger = logger;
    }
    
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await PerformWork(stoppingToken);
    }

    private async Task PerformWork(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await queue.HandleAsync(cancellationToken);
            }
            catch (OperationCanceledException) {}
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception. Waiting 15 seconds before trying again...");
                await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
            }

        }
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
    }
}