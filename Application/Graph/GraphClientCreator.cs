using Application.Configuration;
using Microsoft.Extensions.Options;

namespace Application.Graph;

public class GraphClientCreator
{
    private readonly GraphOptions options;

    public GraphClientCreator(IOptions<GraphOptions> options)
    {
        this.options = options.Value;
    }

    public void CreateClient()
    {
      



    }
}