using System.Text.Json.Serialization;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Application.Graph.Models;

public class RefreshTokenResponse
{
    public required string TokenType { get; init; }
    
    public required string Scope { get; init; }
    
    [JsonPropertyName("expires_in")]
    public required UInt16 ExpiresIn { get; init; }
    
    [JsonPropertyName("ext_expires_in")]
    public required UInt16 ExtensionExpiresIn { get; init; }
    
    [JsonInclude, JsonRequired, JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }
    
    public required string RefreshToken { get; init; }
    
    [JsonPropertyName("id_token")]
    public required string TokenIdentifier { get; init; }
}

