using System.Net;
using System.Security.Authentication;
using System.Text;
using Application.Configuration;
using Application.UserInteractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Graph;

public class Authenticator
{
    private readonly ConfigurationOptions options;
    private readonly GraphService graphService;
    private readonly IMemoryCache cache;
    private readonly ILogger logger;

    internal bool AccessTokenValid { get; set; }
    internal bool RefreshTokenValid { get; set; }

    public Authenticator(GraphService graphService, ConfigurationOptions options, IMemoryCache cache, ILogger logger)
    {
        this.graphService = graphService;
        this.cache = cache;
        this.logger = logger;
        this.options = options;
    }
    
    public async Task AuthenticateAsync()
    {
        await UpdateRefreshAndAccessTokenAsync();
    }
    
    public async Task UpdateRefreshAndAccessTokenAsync()
    {
        string code = UserHandler.GetAuthorizationCodeFromUser(graphService.GetAuthenticationCodeUri());
        
        var result = await graphService.GetRefreshTokenAsync(code);
        if (result.StatusCode != HttpStatusCode.OK || result.Response == null)
            throw new AuthenticationException("Could not get refresh token from code");

        await SaveTokenAsync(TokenType.Refresh, result.Response.RefreshToken);
        await SaveTokenAsync(TokenType.Access, result.Response.AccessToken);
        
        AccessTokenValid = true;
        RefreshTokenValid = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Access token</returns>
    public async Task<string> UpdateAccessTokenAsync()
    {
        string? refreshToken = await GetStoredTokenAsync(TokenType.Refresh);
        if (refreshToken == null)
        {
            await UpdateRefreshAndAccessTokenAsync();
            return GetRequiredStoredToken(TokenType.Access, cache);
        }

        var result = await graphService.GetAccessTokenAsync(refreshToken);
        //could be that the refresh token expired
        if (result.StatusCode.IsSuccessful() == false || result.Response?.AccessToken == null)
        {
            await UpdateRefreshAndAccessTokenAsync();
            return GetRequiredStoredToken(TokenType.Access, cache);
        }

        await SaveTokenAsync(TokenType.Access, result.Response!.AccessToken);
        
        AccessTokenValid = true;
        RefreshTokenValid = true;
        
        return GetRequiredStoredToken(TokenType.Access, cache);
    }
    
    #region Utilities


    private async Task SaveTokenAsync(TokenType tokenType, string content)
    {
        string tokenName = tokenType == TokenType.Access ? "access-token" : "refresh-token";
        
        cache.Set(tokenName, content);
        
        string fullPath = tokenType switch
        {
            TokenType.Refresh => options.RefreshTokenFullPath,
            TokenType.Access => options.AccessTokenFullPath,
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
        };
        
        try
        {
            var streamOptions = new FileStreamOptions
            {
                Options = FileOptions.Encrypted,
                Access = FileAccess.ReadWrite,
                Mode = FileMode.OpenOrCreate,
                Share = FileShare.Read
            };
            
            string directoryName = new FileInfo(fullPath).DirectoryName ?? "/";
            Directory.CreateDirectory(directoryName);
            
            await using var writer = new StreamWriter(fullPath, Encoding.UTF8, streamOptions);
            await writer.WriteAsync(content);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception writing token to file.");
        }
    }
    
    internal async Task<string?> GetStoredTokenAsync(TokenType tokenType)
    {
        string? token = null;
        
        token = GetStoredToken(tokenType, cache);
        if (token != null)
            return token;
        
        string fullPath = tokenType switch
        {
            TokenType.Refresh => options.RefreshTokenFullPath,
            TokenType.Access => options.AccessTokenFullPath,
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
        };
        
        token = await GetStoredTokenAsync(fullPath);
        
        return token;
    }
    
    private static async Task<string?> GetStoredTokenAsync(string fileFullPath)
    {
        if (!File.Exists(fileFullPath)) 
            return null;

        try
        {
            var streamOptions = new FileStreamOptions
            {
                Options = FileOptions.Encrypted | FileOptions.Asynchronous | FileOptions.SequentialScan,
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Share = FileShare.Read
            };
        
            using var reader = new StreamReader(fileFullPath, Encoding.UTF8, true, streamOptions);
            
            string content = await reader.ReadToEndAsync();
        
            return content;
        }
        catch
        {
            return null;
        }
    }
    
    private static string GetRequiredStoredToken(TokenType tokenType, IMemoryCache cache)
    {
        string tokenName = tokenType == TokenType.Access ? "access-token" : "refresh-token";

        cache.TryGetValue(tokenName, out string? token);
        if (token == null)
            throw new KeyNotFoundException($"Could not find find {nameof(tokenType)}");
        
        return token;
    }
    
    private static string? GetStoredToken(TokenType tokenType, IMemoryCache cache)
    {
        string tokenName = tokenType == TokenType.Access ? "access-token" : "refresh-token";
        
        cache.TryGetValue(tokenName, out string? token);
        return token ?? null;
    }

    #endregion
}