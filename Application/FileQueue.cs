using System.Threading.Channels;
using Application.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Channel = System.Threading.Channels.Channel;

namespace Application;

public class FileQueue
{
    public readonly Channel<string> Channel = System.Threading.Channels.Channel.CreateBounded<string>(10);
    private readonly Uploader uploader;
    private readonly UploadOptions options;

    public FileQueue(Uploader uploader, IOptions<UploadOptions> options)
    {
        this.uploader = uploader;
        this.options = options.Value;
    }

    public async Task Add(string fullPath)
    {
        await Channel.Writer.WriteAsync(fullPath);
    }

    public async ValueTask<string> Consume(CancellationToken cancellationToken = default)
    {
        return await Channel.Reader.ReadAsync(cancellationToken);
    }

    public async ValueTask HandleAsync(CancellationToken cancellationToken = default)
    {
        ChannelReader<string> reader = Channel.Reader;
        
        
        
        
        
        while (await reader.WaitToReadAsync(cancellationToken))
        {
            while (reader.TryRead(out string? fullPath))
            {
                if (fullPath == null) continue;

                FileInfo file = new FileInfo(fullPath);
                
                string directoryName = options.DestinationDirectoryName;
                string fileName = file.Name;

                var stream = GetStream(file);
                
                await uploader.UploadAsync(directoryName, fileName, stream);
            }
        }
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