using System.Threading.Channels;
using Application.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Channel = System.Threading.Channels.Channel;

namespace Application;

public class FileQueue
{

    private static readonly BoundedChannelOptions boundedChannelOptions = new(500)
    {
        FullMode = BoundedChannelFullMode.Wait,
    };

    public readonly Channel<string> Channel = System.Threading.Channels.Channel.CreateBounded<string>(boundedChannelOptions);
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
}