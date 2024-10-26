using System.Text.Json.Serialization;

namespace Application.Graph.Models;

internal class UploadSession
{
    [JsonPropertyName("uploadUrl"), JsonInclude]
    internal required string UploadUrl { get; init; }
    
    [JsonPropertyName("expirationDateTime"), JsonInclude]
    internal required DateTime Expiration { get; init; }
}