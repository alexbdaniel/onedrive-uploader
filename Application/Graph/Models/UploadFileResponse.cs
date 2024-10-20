using System.Net;

namespace Application.Graph.Models;

public class UploadFileResponse
{
    public required HttpStatusCode StatusCode { get; init; }
    
    public required string RawBodyContent { get; init; }
    
    
}