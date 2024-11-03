using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Env = System.Environment;



namespace Application.Configuration;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ConfigurationOptions
{
    public const string Key = "Configuration";

    private const string ApplicationFileSystemName = "onedrive-uploader";
    
    [Required] [DirectoryExists]
    public string ApplicationDirectoryName { get; init; } =
        Path.Combine(Env.GetFolderPath(Env.SpecialFolder.LocalApplicationData), ApplicationFileSystemName);
    
    public string ConfigurationDirectoryName { get; init; } =
        Path.Combine(Env.GetFolderPath(Env.SpecialFolder.ApplicationData), ApplicationFileSystemName);
    
    [DirectoryExists(Create = true)]
    public string SecretsDirectoryName =>
        Path.Combine(ApplicationDirectoryName, "secrets");
    
    public string RefreshTokenFullPath =>
        Path.Combine(SecretsDirectoryName, "refresh-token.txt");
    
    public string AccessTokenFullPath =>
        Path.Combine(SecretsDirectoryName, "access-token.txt");
}