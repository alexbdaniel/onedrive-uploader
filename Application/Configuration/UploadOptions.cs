using System.ComponentModel.DataAnnotations;

namespace Application.Configuration;

public class UploadOptions
{
    public const string Key = "Upload";
    
    [Required]
    public required string SourceDirectoryName { get; init; }
    
    [Required]
    public required string DestinationDirectoryName { get; init; }
    
    public bool DeleteAfterUpload { get; init; }
}