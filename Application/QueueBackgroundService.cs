using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Application.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;

namespace Application;

public class QueueBackgroundService : BackgroundService
{
    private readonly FileQueue queue;
    private readonly UploadOptions options;
    private readonly Uploader uploader;

    public QueueBackgroundService(FileQueue queue, IOptions<UploadOptions> options, Uploader uploader)
    {
        this.queue = queue;
        this.uploader = uploader;
        this.options = options.Value;
    }


    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ChannelReader<string> reader = queue.Channel.Reader;
    
            while (await reader.WaitToReadAsync(stoppingToken))
            {
                while (reader.TryRead(out string? fullPath))
                {
                    if (fullPath == null) 
                        continue;

                    FileInfo file = new FileInfo(fullPath);
                    if (!file.Exists)
                        continue;
                    
                    string directoryName = options.DestinationDirectoryName;
                    string fileName = file.Name;

                    await using Stream? stream = await GetStreamAsync(file);
                    if (stream == null)
                        continue;
                    
                    await uploader.UploadAsync(directoryName, fileName, stream);
                }
            }
        }
        
        

    }
    
    private static async Task<Stream?> GetStreamAsync(FileInfo file)
    {
        var streamOptions = new FileStreamOptions
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Share = FileShare.ReadWrite,
            Options = FileOptions.Asynchronous,
        };
        
        UInt16 attempts = 0;
        do
        {
            try
            {
                var stream = new FileStream(file.FullName, streamOptions);
                return stream;
            }
            catch (IOException) 
            { 
                await Task.Delay(200);
            }
           
        } while (++attempts < 50);

        return null;
    }
}