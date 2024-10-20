using System.Text;
using Application.Configuration;
using Application.Graph;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Application;

public class UploadHandler
{
    private readonly GraphService graphService;
    private readonly ConfigurationOptions options;
    private readonly IMemoryCache cache;
    
    public UploadHandler(GraphService graphService, IOptions<ConfigurationOptions> options, IMemoryCache cache)
    {
        this.graphService = graphService;
        this.cache = cache;
        this.options = options.Value;
    }
    
    public async Task HandleAsync()
    {
        var authenticator = new Authenticator(graphService, options, cache);
        await authenticator.UpdateCachedAccessTokenAsync();
        
        

        
    }

}