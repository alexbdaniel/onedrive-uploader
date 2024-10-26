using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Authentication;
using Application.Configuration;
using Application.Graph;
using Application.Graph.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Application;

[SuppressMessage("ReSharper", "MergeIntoLogicalPattern")]
[SuppressMessage("ReSharper", "ConvertIfStatementToNullCoalescingExpression")]
public class Uploader
{
    private readonly GraphService graphService;
    private readonly ConfigurationOptions options;
    private readonly IMemoryCache cache;
    private readonly ILogger logger;

    public Uploader(GraphService graphService, IOptions<ConfigurationOptions> options, IMemoryCache cache, ILogger<Uploader> logger)
    {
        this.graphService = graphService;
        this.cache = cache;
        this.logger = logger;
        this.options = options.Value;
    }

    public async Task UploadAsync(string folderPath, string fileName, Stream stream)
    {
        var authenticator = new Authenticator(graphService, options, cache, logger);

        string? accessToken = await authenticator.GetStoredTokenAsync(TokenType.Access);
        if (accessToken == null)
            accessToken = await authenticator.UpdateAccessTokenAsync();

        var response = await graphService.UploadSmallFileAsync(accessToken, folderPath, fileName, stream);
        if (response.StatusCode.IsSuccessful())
            return;

        if (response.StatusCode.IsUnauthenticatedOrUnauthorized() == false)
        {
            logger.LogError("Unhandled response. StatusCode = \"{response.StatusCode}\"; RawBodyContent = \"{response.RawBodyContent}\"", 
                response.StatusCode, response.RawBodyContent);
            return;
        }
        
        //access token could be old, despite being in cache, so try refresh again. This method handles updating refresh token.
        accessToken = await authenticator.UpdateAccessTokenAsync();

        response = await graphService.UploadSmallFileAsync(accessToken, folderPath, fileName, stream);
        if (response.StatusCode.IsSuccessful())
            return;
        
        logger.LogError("Unhandled response. StatusCode = \"{response.StatusCode}\"; RawBodyContent = \"{response.RawBodyContent}\"", 
            response.StatusCode, response.RawBodyContent);
    }
}