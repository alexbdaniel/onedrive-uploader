using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace Application;

[SuppressMessage("ReSharper", "MergeIntoLogicalPattern")]
public static class Utilities
{
    public static Uri Append(this Uri uri, params string[] paths)
    {
        return new Uri(paths.Aggregate(uri.AbsoluteUri, (current, path) => $"{current.TrimEnd('/')}/{path.TrimStart('/')}"));
    }

    public static readonly JsonSerializerOptions GraphSerializerOptions = new()
    {
        IncludeFields = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static readonly JsonSerializerOptions WriteSerializerOptions = new()
    {
        IncludeFields = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public static void Clear(this MemoryStream source)
    {
        byte[] buffer = source.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);
        source.Position = 0;
        source.SetLength(0);
    }
    
    public static bool IsSuccessful(this HttpStatusCode statusCode) =>
        (int)statusCode >= 200 && (int)statusCode <= 299;

    public static bool IsFailure(this HttpStatusCode statusCode) =>
        statusCode.IsSuccessful() == false;

    public static bool IsUnauthenticatedOrUnauthorized(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden;

    public static bool IsDirectory(this FileInfo file) =>
         (file.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
    
}

public enum TokenType
{
    Access,
    Refresh
}