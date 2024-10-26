using System.Net;

namespace Application.Graph.Models;

public record UploadFileResponse
{
    
    
    
    public HttpStatusCode StatusCode { get; private init; }
    
    public string? RawBodyContent { get; private init; }


    public UploadFileResponse(HttpStatusCode statusCode = HttpStatusCode.Created)
    {
        if (statusCode.IsFailure())
            throw new ArgumentException("Incorrect overload for created response.", nameof(statusCode));
        
        StatusCode = statusCode;
    }
    
    public UploadFileResponse(HttpStatusCode statusCode, string rawBodyContent)
    {
        if (statusCode.IsSuccessful())
            throw new ArgumentException("Incorrect overload for a response other than created.", nameof(statusCode));
        
        StatusCode = statusCode;
        RawBodyContent = rawBodyContent;
    }
    
}