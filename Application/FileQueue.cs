using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;

namespace Application;

public class FileQueue
{
    private static readonly BoundedChannelOptions boundedChannelOptions = new(500)
    {
        FullMode = BoundedChannelFullMode.Wait,
    };

    private readonly Channel<string> Channel = System.Threading.Channels.Channel.CreateBounded<string>(boundedChannelOptions);
    private readonly Uploader uploader;
    private readonly UploadOptions options;
    private readonly ILogger logger;

    public FileQueue(Uploader uploader, IOptions<UploadOptions> options, ILogger<FileQueue> logger)
    {
        this.uploader = uploader;
        this.logger = logger;
        this.options = options.Value;
    }
    
    public async Task Add(string fullPath)
    {
        await Channel.Writer.WriteAsync(fullPath);
    }
    
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
    public async ValueTask HandleAsync(CancellationToken cancellationToken = default)
    {
        ChannelReader<string> reader = Channel.Reader;
        
        while (await reader.WaitToReadAsync(cancellationToken))
        {
            while (reader.TryRead(out string? fullPath))
            {
                if (fullPath == null) 
                    continue;
            
                FileInfo file = new FileInfo(fullPath);
                if (!file.Exists)
                    continue;
            
                await using Stream? stream = await GetStreamAsync(file);
                if (stream == null)
                    continue;
            
                var (destinationDirectoryName, fileName) = GetDestinationPaths(file);
            
                bool result = await uploader.UploadAsync(destinationDirectoryName, fileName, stream);
                await HandleResultAsync(result, stream, file);
            
                Console.WriteLine($"Items remaning: {Channel.Reader.Count}.");
            }
        }
    }
    
    private (string, string) GetDestinationPaths(FileInfo file)
    { 
        string sourceRootDirectoryName = new Uri(options.SourceDirectoryName).LocalPath;
        string sourceRelativePath = file.FullName.Replace(sourceRootDirectoryName, ""); //includes fileName

        int namePosition = sourceRelativePath.LastIndexOf(file.Name, StringComparison.Ordinal);

        string sourceRelativeDirectoryName = sourceRelativePath[..namePosition];
        string destinationDirectoryName = Path.Combine(options.DestinationDirectoryName, sourceRelativeDirectoryName);
        
        return (destinationDirectoryName, file.Name);
    }



    private async Task HandleResultAsync(bool result, Stream stream, FileInfo file)
    {
        if (result == false)
            return;

        if (options.DeleteAfterUpload == false)
            return;

        try
        {
            stream.Close(); 
            UInt16 attempts = 0;
            do
            {
                try
                {
                    file.Delete();
                    return;
                }
                catch (IOException)
                {
                    await Task.Delay(200);
                }
            } while (++attempts < 20);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while handling upload result.");
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