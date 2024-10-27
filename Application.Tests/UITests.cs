using Application.UserInteractions;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Application.Tests;

[TestSubject(typeof(UserHandler))]
public class UITests
{
    private readonly ITestOutputHelper testConsole;

    public UITests(ITestOutputHelper testConsole)
    {
        this.testConsole = testConsole;
    }

    [Theory]
    [InlineData("https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=a54ddc56-2c78-497c-bdbf-f99a9aa53c7a&response_type=code&redirect_uri=http://localhost&scope=openid profile Files.ReadWrite.All offline_access")]
    public void PromptForAuthTemp(string url)
    {
       string result = UserHandler.GetAuthorizationCodeFromUser(url);
       testConsole.WriteLine(result);
    }
}