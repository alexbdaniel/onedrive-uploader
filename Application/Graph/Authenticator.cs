using System.Net;
using System.Security.Authentication;
using System.Text;
using Application.Configuration;
using Application.Graph.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Application.Graph;

public class Authenticator
{
    private readonly ConfigurationOptions options;
    private readonly GraphService graphService;
    private readonly IMemoryCache cache;

    public Authenticator(GraphService graphService, ConfigurationOptions options, IMemoryCache cache)
    {
        this.graphService = graphService;
        this.cache = cache;
        this.options = options;
    }
    
    public Authenticator(GraphService graphService, IOptions<ConfigurationOptions> configOptions, IMemoryCache cache)
    {
        this.graphService = graphService;
        this.cache = cache;
        this.options = configOptions.Value;
    }

    public async Task<bool> CheckRefreshTokenExists()
    {
        string? token = await GetStoredTokenAsync(options.RefreshTokenFullPath, TokenType.Refresh, cache);
        return string.IsNullOrWhiteSpace(token);
    }
    
    public async Task AuthenticateAsync()
    {
        string? accessToken = await GetStoredTokenAsync(options.AccessTokenFullPath, TokenType.Access, cache);
        string? refreshToken = await GetStoredTokenAsync(options.RefreshTokenFullPath, TokenType.Refresh, cache);
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            string code = UserHandler.GetAuthorizationCodeFromUser(graphService.GetAuthenticationCodeUri());
            await GetRefreshTokenAsync(code); //gets refresh + access tokens and saves to cache
            return;
        }
        
        var (statusCode, accessTokenResponse) = await graphService.GetAccessTokenAsync(refreshToken);
        //handle if refresh token is expired too
        if (!statusCode.IsSuccessful() || accessTokenResponse == null)
        {
            string code = UserHandler.GetAuthorizationCodeFromUser(graphService.GetAuthenticationCodeUri());
            await GetRefreshTokenAsync(code); //gets refresh + access tokens and saves to cache
            return;
        }
        
        await SaveTokenAsync(options.AccessTokenFullPath, TokenType.Access, accessTokenResponse.AccessToken, cache);
    }



    /// <summary>
    /// Gets refresh and access tokens and saves them to the cache.
    /// </summary>
    public async Task UpdateRefreshTokenAsync()
    {
        string code = UserHandler.GetAuthorizationCodeFromUser(graphService.GetAuthenticationCodeUri());
        await GetRefreshTokenAsync(code); 
    }
    
    

    /// <summary>
    /// Gets refresh token and adds it to cache.
    /// </summary>
    /// <param name="code">Authorization code from user. Authorizes application.</param>
    public async Task GetRefreshTokenAsync(string code)
    {
        (HttpStatusCode, RefreshTokenResponse?) result = await graphService.GetRefreshTokenAsync(code);
        if (result.Item1 != HttpStatusCode.OK || result.Item2 == null)
            throw new AuthenticationException("Could not get refresh token from code");

        await SaveTokenAsync(options.RefreshTokenFullPath, TokenType.Refresh, result.Item2.RefreshToken, cache);
        await SaveTokenAsync(options.AccessTokenFullPath, TokenType.Access, result.Item2.AccessToken, cache);
    }
    
    
    public async Task<AuthenticationResult> UpdateCachedAccessTokenAsync(bool force = false)
    {
        cache.TryGetValue("access-token", out string? accessToken);
        
        string? refreshToken = await GetStoredTokenAsync(options.RefreshTokenFullPath, TokenType.Refresh, cache);
        if (refreshToken == null)
            return AuthenticationResult.NewRefreshTokenRequired;
        
        var refreshTokenResponse = await graphService.GetAccessTokenAsync(refreshToken);

        if (refreshTokenResponse.Item1 != HttpStatusCode.OK)
            throw new AuthenticationException();
        
        
        cache.Set("access-token", refreshTokenResponse!.Item2!.AccessToken);
        cache.Set("refresh-token", refreshTokenResponse!.Item2!.RefreshToken);

        return AuthenticationResult.Success;
    }
    
    #region Refresh token file


    private async Task SaveTokenAsync(TokenType tokenType, string content)
    {
        string fullPath = tokenType switch
        {
            TokenType.Refresh => options.RefreshTokenFullPath,
            TokenType.Access => options.AccessTokenFullPath,
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
        };
        
        await SaveTokenAsync(fullPath, tokenType, content, cache);
    }
    
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
    
    /// <summary>
    /// First attempts to get new access token, then attempts to get new refresh token.
    /// </summary>
    public async Task UpdateTokensAsync()
    {
        string refreshToken = await GetTokenAsync(TokenType.Refresh);
        var (statusCode, response) = await graphService.GetAccessTokenAsync(refreshToken);
        if (statusCode != HttpStatusCode.OK || response?.AccessToken == null)
        {
            await UpdateRefreshTokenAsync();
            return;
        }

        await SaveTokenAsync(TokenType.Access, response.AccessToken);
    }
    
    public async Task<string> GetTokenAsync(TokenType tokenType)
    {
        if (tokenType == TokenType.Access)
        {
            string? accessToken = await GetStoredTokenAsync(options.AccessTokenFullPath, TokenType.Access, cache);
            if (accessToken != null) return accessToken;
            await AuthenticateAsync();
            accessToken = await GetStoredTokenAsync(options.AccessTokenFullPath, TokenType.Access, cache);
            if (accessToken == null) throw new AuthenticationException($"{nameof(accessToken)} is null. Could not get it.");
            return accessToken;
        }
        
        string? refreshToken = await GetStoredTokenAsync(options.RefreshTokenFullPath, TokenType.Refresh, cache);
        if (refreshToken != null) return refreshToken;
        await AuthenticateAsync();
        refreshToken = await GetStoredTokenAsync(options.RefreshTokenFullPath, TokenType.Refresh, cache);
        if (refreshToken == null) throw new AuthenticationException($"{nameof(refreshToken)} is null. Could not get it.");
        return refreshToken;
    }
    

    public static async Task<string?> GetStoredTokenAsync(string fileFullPath, TokenType tokenType, IMemoryCache? cache = null)
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
            Options = FileOptions.Encrypted | FileOptions.Asynchronous | FileOptions.SequentialScan,
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Share = FileShare.None
        };
        
        using var reader = new StreamReader(fileFullPath, Encoding.UTF8, true, streamOptions);
        
        string content = await reader.ReadToEndAsync();

        cache?.Set(tokenName, content);

        return content;
    }
    
    
   

    #endregion
}