using System.Net;

namespace Application.Graph.Models;

public class GraphServiceResult<TResponseModel> where TResponseModel : class
{
    public HttpStatusCode StatusCode { get; private init; }
    
    public TResponseModel? Response { get; private init; }
    
    public string? Message { get; private init; }


    public GraphServiceResult(HttpStatusCode statusCode, string message)
    {
        if (statusCode.IsSuccessful())
            throw new ArgumentException("This overload is only for failure responses.");
        
        StatusCode = statusCode;
        Message = message;
    }

    public GraphServiceResult(HttpStatusCode statusCode, TResponseModel response)
    {
        if (statusCode.IsFailure())
            throw new ArgumentException("This overload is only for successful responses");
        
        StatusCode = statusCode;
        Response = response;
    }
}