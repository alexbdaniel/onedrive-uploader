using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Configuration;
using Application.Graph.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Application.Graph;

public class GraphService
{
    private readonly HttpClient client;
    private readonly GraphOptions options;
    
    public GraphService(HttpClient client, IOptions<GraphOptions> options)
    {
        this.client = client;
        this.options = options.Value;
    }
    
    /// <summary>
    /// Authentication code for interactive logon.
    /// </summary>
    /// <returns></returns>
    public string GetAuthenticationCodeUri()
    {
        Dictionary<string, string?> parameters = new()
        {
            { "client_id", options.ClientId },
            {"response_type", "code"},
            {"redirect_uri", options.RedirectUri},
            {"scope", "openid profile Files.ReadWrite.All offline_access"}
        };

        string uri = QueryHelpers.AddQueryString(options.AuthorizeUri, parameters);

        return uri;
        
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code">The authentication code from the redirected uri.</param>
    /// <returns></returns>
    public async Task<(HttpStatusCode, RefreshTokenResponse?)> GetRefreshTokenAsync(string code)
    {
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", options.ClientId),
            new KeyValuePair<string, string>("client_secret", options.ClientSecret),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", options.RedirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("scope", "Files.ReadWrite.All offline_access")
        });
        
        var message = new HttpRequestMessage(HttpMethod.Post, options.AccessTokenUri);
        message.Content = formContent;
        
        var response = await client.SendAsync(message);

        string raw = await response.Content.ReadAsStringAsync();
        
        RefreshTokenResponse? content = null;
        if (response.StatusCode == HttpStatusCode.OK)
            content = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>(Utilities.GraphSerializerOptions);
        
        return (response.StatusCode, content);
    }
    
    
    public async Task<(HttpStatusCode, AccessTokenResponse?)> GetAccessTokenAsync(string refreshToken)
    {
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", options.ClientId),
            new KeyValuePair<string, string>("client_secret", options.ClientSecret),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("redirect_uri", options.RedirectUri),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("scope", "Files.ReadWrite.All offline_access")
        });
        
        var message = new HttpRequestMessage(HttpMethod.Post, options.AccessTokenUri);
        message.Content = formContent;
        
        var response = await client.SendAsync(message);
        
        AccessTokenResponse? content = null;
        if (response.StatusCode == HttpStatusCode.OK)
            content = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(Utilities.GraphSerializerOptions);
        
        return (response.StatusCode, content);
    }

    public async Task<UploadFileResponse> UploadSmallFileAsync(string accessToken, string destination, Stream contents)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        
        Uri prePath = options.BaseUri.Append(options.Endpoints.FileUpload);

        Uri full = new Uri($"{prePath.AbsoluteUri}:{destination}:/content");

        var message = new HttpRequestMessage(HttpMethod.Put, full);
        message.Content = new StreamContent(contents);
        
        var response = await client.SendAsync(message);

        string rawContent = await response.Content.ReadAsStringAsync();

        return new UploadFileResponse
        {
            StatusCode = response.StatusCode,
            RawBodyContent = rawContent
        };
    }
    

    public async Task<bool> TestAuthenticationWithBearerTokenAsync(string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        Uri uri = options.BaseUri.Append("/me/");
        
        var response = await client.GetAsync(uri);

        var content = await response.Content.ReadAsStringAsync();
        
        return response.StatusCode == HttpStatusCode.OK;
    }
}