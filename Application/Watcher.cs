using System.Diagnostics.CodeAnalysis;
using Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application;

[SuppressMessage("ReSharper", "UsingStatementResourceInitialization")]
public class Watcher
{
    private readonly FileQueue queue;
    private readonly UploadOptions options;
    private readonly ILogger logger;
    // private readonly Uploader uploader;

    public Watcher(IOptions<UploadOptions> options, FileQueue queue, ILogger<Watcher> logger)
    {
        this.options = options.Value;
        this.queue = queue;
        this.logger = logger;
    }

    public Task Watch(CancellationToken cancellationToken = default)
    {

        
        var watcher = new FileSystemWatcher()
        {
            Path = options.SourceDirectoryName,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName
                           | NotifyFilters.LastWrite,
        };
        
        watcher.Created += OnCreatedAsync;
        watcher.Renamed += OnCreatedAsync;
        watcher.Changed += OnCreatedAsync;
        
        var completionSource = new TaskCompletionSource();
        _ = cancellationToken.Register(() => completionSource.TrySetResult());
        
        return completionSource.Task;
    }

    public async Task HandleInitialFiles()
    {
        var directory = new DirectoryInfo(options.SourceDirectoryName);
        FileInfo[] files = directory.GetFiles();

        foreach (FileInfo file in files)
        {
            await queue.Add(file.FullName);
        }
    }
    
    private async void OnCreatedAsync(object sender, FileSystemEventArgs e)
    {
        await queue.Add(e.FullPath);
        // logger.LogInformation("New file added to queue \"{fileCreatedFullPath}\"", e.FullPath);
    }
}