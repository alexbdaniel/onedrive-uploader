using System.Threading.Tasks;
using Application.Graph;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Graph;

[TestSubject(typeof(Authenticator))]
public class AuthenticatorTest
{

    [Fact]
    public async Task METHOD()
    {
        await using var provider = new TestServiceProvider().GetTestServiceProvider();

        var authenticator = provider.GetRequiredService<Authenticator>();

        await authenticator.UpdateCachedAccessTokenAsync();


    }
}