using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Application.Configuration;

[SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
public class FileExists : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    { 
        string? path = value?.ToString();

        bool exists = File.Exists(path);

        if (exists)
            return ValidationResult.Success;
        
        return new ValidationResult($"File does not exist at \"{path}\"");
    }
}

[SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
public class DirectoryExists : ValidationAttribute
{
    /// <summary>
    /// Create the directory if it does not exist.
    /// </summary>
    public bool Create { get; set; }
    
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    { 
        string? path = value?.ToString();
        if (path == null)
            return new ValidationResult($"Path supplied is empty.");
        
        bool exists = Directory.Exists(path);

        if (exists)
            return ValidationResult.Success;
        
        if (!exists && !Create)
            return new ValidationResult($"Directory does not exist at \"{path}\"");
        
        var directory = Directory.CreateDirectory(path);
        
        return ValidationResult.Success;
    }
}