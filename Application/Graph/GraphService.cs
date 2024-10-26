using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using Application.Configuration;
using Application.Graph.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using UploadSession = Application.Graph.Models.UploadSession;

namespace Application.Graph;

[SuppressMessage("ReSharper", "MergeIntoLogicalPattern")]
public class GraphService
{
    private readonly HttpClient client;
    private readonly GraphOptions options;
    private readonly ILogger logger;
    
    public GraphService(HttpClient client, IOptions<GraphOptions> options, ILogger<GraphService> logger)
    {
        this.client = client;
        this.logger = logger;
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
    /// <returns>Refresh token returned if status is successful.</returns>
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




    #region Large file

    public async Task<bool> UploadLargeFileAsync(string accessToken, string destination, Stream contents, ConflictBehavior onConflict)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var session = await CreateUploadSessionAsync(accessToken, destination, ConflictBehavior.Rename);
        Uri uri = new Uri(session.UploadUrl);
     

        byte[] buffer = new byte[26];
        int read;

        var memoryStream = new MemoryStream(buffer, 0,buffer.Length,true, true);

        
        long size = contents.Length;
        contents.Position = 0;
        while ((read = await contents.ReadAsync(buffer)) > 0)
        {

            memoryStream.Position = 0;
            memoryStream.SetLength(0);
            memoryStream.Write(buffer);
            memoryStream.Position = 0;

            var requestContent = new StreamContent(memoryStream, buffer.Length);
            
            long to = contents.Position - 1;
            long from = contents.Position - buffer.Length;

            var message = new HttpRequestMessage(HttpMethod.Put, uri);
            message.Content = requestContent;
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentLength = buffer.Length;
            requestContent.Headers.ContentRange = new ContentRangeHeaderValue(from, to,size);
            
            var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);

            string rawContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(rawContent);

            response.EnsureSuccessStatusCode();
        }
        
        return true;
    }



    

    private async Task<UploadSession> CreateUploadSessionAsync(string accessToken, string destination, ConflictBehavior onConflict = ConflictBehavior.Rename)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        Uri prePath = options.BaseUri.Append(options.Endpoints.FileUpload);
        Uri full = new Uri($"{prePath.AbsoluteUri}:{destination}:/createUploadSession");

        var body = new UploadSessionRequestBody { Item = new UploadSessionRequestBody.ItemContents { ConflictBehavior = ConflictBehavior.Rename } };

        var message = new HttpRequestMessage(HttpMethod.Post, full);
        string txt = JsonSerializer.Serialize(body, new JsonSerializerOptions{ IncludeFields = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        message.Content = new StringContent(txt, new MediaTypeHeaderValue("application/json", "UTF-8"));

        var response = await client.SendAsync(message);

        var raw = await response.Content.ReadAsStringAsync();

        var session = await response.Content.ReadFromJsonAsync<UploadSession>();
        if (session == null) throw new SerializationException($"Failed to deserialize {nameof(UploadSession)}");
        
        return session;
    }
    

    #endregion

    public async Task<UploadFileResponse> UploadSmallFileAsync(string accessToken, string fullPath, Stream contents)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        Uri prePath = options.BaseUri.Append(options.Endpoints.FileUpload);
        Uri full = new Uri($"{prePath.AbsoluteUri}:{fullPath}:/content");

        using var fileContent = new StreamContent(contents);
        string mimeType = MimeTypes.GetMimeType(Path.GetFileName(fullPath));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

        var args = new UploadSessionRequestBody(ConflictBehavior.Rename);
        
        using var message = new HttpRequestMessage(HttpMethod.Put, full);
        message.Content = new StreamContent(contents);
        
        var response = await client.SendAsync(message);
        if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK) 
            return new UploadFileResponse();

        string rawContent = await response.Content.ReadAsStringAsync();
        return new UploadFileResponse(response.StatusCode, rawContent);
    }
    
    
    public async Task<UploadFileResponse> UploadSmallFileAsync(string accessToken, string folderPath, string fileName, Stream contents)
    {
        string fullPath = Path.Combine(folderPath, fileName);
        return await UploadSmallFileAsync(accessToken, fullPath, contents);
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