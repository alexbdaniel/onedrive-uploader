using System.Text.Json.Serialization;

namespace Application.Graph.Models;

public class AccessTokenResponse
{
    public required string TokenType { get; init; }
    
    public required string Scope { get; init; }
    
    [JsonPropertyName("expires_in")]
    public required UInt16 ExpiresIn { get; init; }
    
    [JsonPropertyName("ext_expires_in")]
    public required UInt16 ExtensionExpiresIn { get; init; }
    
    public required string AccessToken { get; init; }
    
    public required string RefreshToken { get; init; }
}
