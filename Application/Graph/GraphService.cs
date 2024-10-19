using System.Net;
using System.Net.Http.Headers;
using Application.Configuration;
using Microsoft.Extensions.Options;

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

    public async Task<bool> TestAuthenticationWithBearerTokenAsync(string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Uri uri = options.BaseUri.Append("/me/");
        
        var response = await client.GetAsync(uri);

        var content = await response.Content.ReadAsStringAsync();
        
        return response.StatusCode == HttpStatusCode.OK;
    }
}