using Application.Configuration;
using Application.Graph;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Application.UserInteractions;

public class UserHandler
{
    private readonly GraphService graphService;
    private readonly ConfigurationOptions options;
    private readonly IMemoryCache cache;
    
    public UserHandler(GraphService graphService, IOptions<ConfigurationOptions> options, IMemoryCache cache)
    {
        this.graphService = graphService;
        this.cache = cache;
        this.options = options.Value;
    }
    
    public static string GetAuthorizationCodeFromUser(string requestUri)
    {
        string message1 =
            "1. Visit the URI below. \n" +
            "2. Authorize this application with the appropriate permissions.\n" +
            "3. After authorizing, supply the redirect URI.\n" +
            "Visit:\n" +
            $"{requestUri}";
        
        Console.Clear();
        bool valid = false;

        string? authenticationCodeUri = null;
        
        do
        {
            Console.WriteLine(message1);
            
            authenticationCodeUri = Console.ReadLine() ?? "";
            valid = ValidateUri(authenticationCodeUri);
            
        } while (!valid);

        string code = authenticationCodeUri.Replace("http://localhost/?code=", "");
        
        return code;
        
        static bool ValidateUri(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            try
            {
                _ = new Uri(value);
                return true;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }
    }

}