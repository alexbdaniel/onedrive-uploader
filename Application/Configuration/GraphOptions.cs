using System.ComponentModel.DataAnnotations;

namespace Application.Configuration;

public class GraphOptions
{
    public const string Key = "MicrosoftGraph";
    
    [Required]
    public required string BaseAddress { get; init; }
    
    [Required]
    public required string ClientId { get; init; }
    
    [Required]
    public required string ClientSecret { get; init; }
    
    [Required]
    public required string TenantId { get; init; }
    
    [Required]
    public required string RedirectUri { get; init; }
    
    [Required]
    public required string AccessTokenUri { get; init; }
    
    [Required]
    public required string AuthorizeUri { get; init; }
    
    public Uri BaseUri => new Uri(BaseAddress);
    
    [Required]
    public required GraphEndpoints Endpoints { get; init; }
}

public class GraphEndpoints
{
    [Required]
    public required string FileUpload { get; init; }
}
