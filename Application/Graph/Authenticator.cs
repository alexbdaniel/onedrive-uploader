using System.Diagnostics;
using System.Net;
using System.Security.Authentication;
using System.Text;
using Application.Configuration;
using Application.Graph.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Application.Graph;

public class Authenticator
{
    private readonly ConfigurationOptions configOptions;
    private readonly GraphService graphService;
    private readonly IMemoryCache cache;

    public Authenticator(GraphService graphService, ConfigurationOptions configOptions, IMemoryCache cache)
    {
        this.graphService = graphService;
        this.cache = cache;
        this.configOptions = configOptions;
    }
    
    public Authenticator(GraphService graphService, IOptions<ConfigurationOptions> configOptions, IMemoryCache cache)
    {
        this.graphService = graphService;
        this.cache = cache;
        this.configOptions = configOptions.Value;
    }
    
    public async Task<AuthenticationResult> UpdateCachedAccessTokenAsync(bool force = false)
    {
        cache.TryGetValue("access-token", out JsonWebToken? accessToken);

        bool expired = accessToken?.ValidTo < DateTime.UtcNow;
        if (!expired && !force) return AuthenticationResult.Success;
        
        string? refreshToken = await GetTokenAsync(configOptions.RefreshTokenFullPath, TokenType.Refresh, cache);
        if (refreshToken == null)
            throw new AuthenticationException("New refresh token required");
        
        var refreshTokenResponse = await graphService.GetAccessTokenAsync(refreshToken);

        if (refreshTokenResponse.Item1 != HttpStatusCode.OK)
            throw new AuthenticationException();
        
        
        cache.Set("access-token", refreshTokenResponse!.Item2!.AccessToken);
        cache.Set("refresh-token", refreshTokenResponse!.Item2!.RefreshToken);

        return AuthenticationResult.Success;
    }
    
   
    
    #region Refresh token file

   
    
    
    public static async Task SaveTokenAsync(string fileFullPath, TokenType tokenType, string content, IMemoryCache? cache = null)
    {
        var streamOptions = new FileStreamOptions
        {
            Options = FileOptions.Encrypted,
            Access = FileAccess.ReadWrite,
            Mode = FileMode.OpenOrCreate,
            Share = FileShare.None
        };

        await using var writer = new StreamWriter(fileFullPath, Encoding.UTF8, streamOptions);
        await writer.WriteAsync(content);
        
        string tokenName = tokenType == TokenType.Access ? "access-token" : "refresh-token";
        
        cache?.Set(tokenName, content);
    }

    public static async Task<string?> GetTokenAsync(string fileFullPath, TokenType tokenType, IMemoryCache? cache = null)
    {
        string tokenName = tokenType == TokenType.Access ? "access-token" : "refresh-token";
        if (cache != null)
        {
            cache.TryGetValue(tokenName, out string? token);
            if (token != null)
                return token;
        }
        
        if (!File.Exists(fileFullPath)) return null;
        
        var streamOptions = new FileStreamOptions
        {
            Options = FileOptions.Encrypted,
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Share = FileShare.None
        };
        
        using var reader = new StreamReader(fileFullPath, Encoding.UTF8, true, streamOptions);
        
        string? content = await reader.ReadToEndAsync();

        cache?.Set(tokenName, content);

        return content;
        
    }
    
    
   

    #endregion
}