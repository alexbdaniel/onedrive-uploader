using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Application.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;

namespace Application;

[SuppressMessage("ReSharper", "UsingStatementResourceInitialization")]
public class Watcher
{
    private readonly FileQueue queue;
    private readonly UploadOptions options;
    // private readonly Uploader uploader;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public Watcher(IOptions<UploadOptions> options, IServiceScopeFactory serviceScopeFactory, FileQueue queue)
    {
        this.options = options.Value;
        this.serviceScopeFactory = serviceScopeFactory;
        this.queue = queue;
    }

    public Task Watch(CancellationToken cancellationToken = default)
    {
        bool exits = Directory.Exists(options.SourceDirectoryName);
        
        var watcher = new FileSystemWatcher()
        {
            Path = options.SourceDirectoryName,
            NotifyFilter = NotifyFilters.CreationTime
                            | NotifyFilters.DirectoryName
                            | NotifyFilters.FileName,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };
        
        
        watcher.Created += OnCreatedAsync;

        TaskCompletionSource completionSource = new TaskCompletionSource();
        _ = cancellationToken.Register(() =>
        {
            //watcher.Dispose();
            completionSource.TrySetResult();
        });

        return completionSource.Task;
    }
    
    private async void OnCreatedAsync(object sender, FileSystemEventArgs e)
    {
        await queue.Add(e.FullPath);

        // await ProcessQueue("f");



    }

    private static Stream GetStream(FileInfo file)
    {
        var streamOptions = new FileStreamOptions()
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Share = FileShare.ReadWrite,
            Options = FileOptions.Asynchronous,
        };
        
        var stream = new FileStream(file.FullName, streamOptions);
        return stream;
    }
}