using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Configuration;
using Application.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using Xunit.Abstractions;

namespace Application.Tests.Graph;

public class GraphServiceTest
{
    private readonly ITestOutputHelper testConsole;

    public GraphServiceTest(ITestOutputHelper testConsole)
    {
        this.testConsole = testConsole;
    }

    [Theory]
    [InlineData("eyJ0eXAiOiJKV1QiLCJub25jZSI6Il9JT3UtQnZOZzJoLVM5aldWZTYxMWoxakc4ZF9YcjVaNy1hWTFzRldVaG8iLCJhbGciOiJSUzI1NiIsIng1dCI6IjNQYUs0RWZ5Qk5RdTNDdGpZc2EzWW1oUTVFMCIsImtpZCI6IjNQYUs0RWZ5Qk5RdTNDdGpZc2EzWW1oUTVFMCJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9hZWQxNDRlMy1hM2ZlLTQzZjctOWQ4ZS0xYTc0ZTM2ZDhmMWMvIiwiaWF0IjoxNzI5MzIyODE2LCJuYmYiOjE3MjkzMjI4MTYsImV4cCI6MTcyOTMyNzQ3MywiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFWUUFxLzhZQUFBQUk5WHNtc3NSS3JCR3lic0RyOEg3aXFvU2lqR1hvbmltcDRQUGlDaHRRTm0wdDQ0eDZtQXhhcjhsVHR1c2tWVHZVeFBobnVrUmJOQVpERVArUmtYSDVpdHkwM01wS2tMNmVENWRkd3B5MHprPSIsImFtciI6WyJwd2QiLCJtZmEiXSwiYXBwX2Rpc3BsYXluYW1lIjoiT25lRHJpdmVVcGxvYWRlciIsImFwcGlkIjoiYTU0ZGRjNTYtMmM3OC00OTdjLWJkYmYtZjk5YTlhYTUzYzdhIiwiYXBwaWRhY3IiOiIxIiwiZmFtaWx5X25hbWUiOiJEYW5pZWwiLCJnaXZlbl9uYW1lIjoiQWxleCIsImlkdHlwIjoidXNlciIsImlwYWRkciI6IjQ5LjE5NC45OS4xNyIsIm5hbWUiOiJEYW5pZWwsIEFsZXgiLCJvaWQiOiI5MGJhMjMyMi04MGY2LTQyZWYtOWU1ZS1mMTFiMmIzZDNhNjEiLCJwbGF0ZiI6IjMiLCJwdWlkIjoiMTAwMzIwMDM1RjE1OUQ2RSIsInJoIjoiMC5BV1lBNDBUUnJ2Nmo5ME9kamhwMDQyMlBIQU1BQUFBQUFBQUF3QUFBQUFBQUFBQ2pBRnMuIiwic2NwIjoiRmlsZXMuUmVhZC5BbGwgRmlsZXMuUmVhZFdyaXRlLkFsbCBVc2VyLlJlYWQgcHJvZmlsZSBvcGVuaWQgZW1haWwiLCJzaWduaW5fc3RhdGUiOlsia21zaSJdLCJzdWIiOiJuVE13VndsZFVkcHFLVjhHLVNKLWlRd3M4NjF6M0gwYWt1bkFDeFRfNWhRIiwidGVuYW50X3JlZ2lvbl9zY29wZSI6Ik9DIiwidGlkIjoiYWVkMTQ0ZTMtYTNmZS00M2Y3LTlkOGUtMWE3NGUzNmQ4ZjFjIiwidW5pcXVlX25hbWUiOiJBbGV4LkRhbmllbEBhbGV4YmRhbmllbC5jb20iLCJ1cG4iOiJBbGV4LkRhbmllbEBhbGV4YmRhbmllbC5jb20iLCJ1dGkiOiJia3NSMDdlLUJFTy1YcV9jRG40akFBIiwidmVyIjoiMS4wIiwid2lkcyI6WyI2MmU5MDM5NC02OWY1LTQyMzctOTE5MC0wMTIxNzcxNDVlMTAiLCJiNzlmYmY0ZC0zZWY5LTQ2ODktODE0My03NmIxOTRlODU1MDkiXSwieG1zX2lkcmVsIjoiNiAxIiwieG1zX3N0Ijp7InN1YiI6IkJLOG94RG9IMUZ4a3A1cUw2NEtnMGZjeW9SVWVPZU9Rcm9XSnF2cDJxdDQifSwieG1zX3RjZHQiOjE3MDk5NjU1NjZ9.M_eeZctm4Ae4NpGnb7cv5Pz07qNKo9TkYLewt89x2qrvb2Gy-RFe3Kg8OOzVoGJzV7ojPlGHpRg2z52-Vg1_84tdCjUk_VFDeBJ1mTTlwgE4Yvgj6-8zM_JmgRNm1UQSKIdmiQs96sABkV2KZyCtqo5X5f9qKhu256ao7poAsmGh-ptC7BpWFaCYSpJvovbsiMToM96njbg0KkfjpKmwLEPNFG9NkCPcoAWUKp4OqJrxUVazHhlxMmir_XoIsfouChClDDdnAoiP29vM8RSmH0gMp6e1Fm_qcFcYTiIv7S_AZYa42m9zIWFjsrh6dgE0iDkLCFyuUecMjknNO9YOpw")]
    public async Task GraphApiWorks(string token)
    {
        await using var provider = new TestServiceProvider().GetTestServiceProvider();

        var graphService = provider.GetRequiredService<GraphService>();
        
        await graphService.TestAuthenticationWithBearerTokenAsync(token);
    }

    [Fact]
    public async Task GetAccessToken()
    {
        await using var provider = new TestServiceProvider().GetTestServiceProvider();

        var options = provider.GetRequiredService<IOptions<ConfigurationOptions>>().Value;

        using var reader = new StreamReader(options.RefreshTokenFullPath, Encoding.UTF8);
        string refreshToken = await reader.ReadToEndAsync();
        reader.Close();
        
        var graphService = provider.GetRequiredService<GraphService>();
        
        var response = await graphService.GetAccessTokenAsync(refreshToken);

        string accessToken = response.Item2.AccessToken;
        await Authenticator.SaveTokenAsync(options.AccessTokenFullPath, TokenType.Access, accessToken);

    }

    [Fact]
    public async Task Small_file_uploads_replacing_existing()
    {
        //configuration
        await using var provider = new TestServiceProvider().GetTestServiceProvider();
        var graphService = provider.GetRequiredService<GraphService>();
        var options = provider.GetRequiredService<IOptions<ConfigurationOptions>>().Value;
        
        string accessToken = await Authenticator.GetTokenAsync(options.AccessTokenFullPath, TokenType.Access);
        if (accessToken == null)
            throw new NullReferenceException(nameof(accessToken));

        string temporaryFileFullPath = $@"C:\Users\{Environment.UserName}\projects\TestFiles\OneDriveUploader\testcontent.txt";
        FileInfo temporaryFile = TestUtilities.CreateTemporaryFileWithContent(temporaryFileFullPath);
        using var reader = new StreamReader(temporaryFile.FullName);

        string cloudPath = $"/TestFiles/{temporaryFile.Name}";
        
        var response = await graphService.UploadSmallFileAsync(accessToken, cloudPath, reader.BaseStream);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        reader.Close();
        //cleanup
        temporaryFile.Delete();
    }

    [Fact]
    public async Task Small_file_uploads_with_201()
    {
        //configuration
        await using var provider = new TestServiceProvider().GetTestServiceProvider();
        var graphService = provider.GetRequiredService<GraphService>();
        var options = provider.GetRequiredService<IOptions<ConfigurationOptions>>().Value;
        
        string accessToken = await Authenticator.GetTokenAsync(options.AccessTokenFullPath, TokenType.Access);
        if (accessToken == null)
            throw new NullReferenceException(nameof(accessToken));
        
        FileInfo temporaryFile = TestUtilities.CreateTemporaryFileWithContent();
        using var reader = new StreamReader(temporaryFile.FullName);

        string cloudPath = $"/TestFiles/{temporaryFile.Name}";
        
        var response = await graphService.UploadSmallFileAsync(accessToken, cloudPath, reader.BaseStream);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        reader.Close();
        //cleanup
        temporaryFile.Delete();
    }

    

    [Fact]
    public async Task GetAuthCode()
    {
        await using var provider = new TestServiceProvider().GetTestServiceProvider();
        var graphService = provider.GetRequiredService<GraphService>();

        var response = graphService.GetAuthenticationCodeUri();
        
        System.Diagnostics.Debug.WriteLine(response);
        
        testConsole.WriteLine(response);
        
        Assert.True(true, response);
    }

    [Theory]
    [InlineData("M.C544_BAY.2.U.6bbcd4f6-b755-0a13-f8c2-a0634e9dfe4b")]
    public async Task GetRefreshTokenFromCode(string authenticationCode)
    {
        await using var provider = new TestServiceProvider().GetTestServiceProvider();
        var graphService = provider.GetRequiredService<GraphService>();

        var response = await graphService.GetRefreshTokenAsync(authenticationCode);

        string txt = JsonSerializer.Serialize(response.Item2);
        testConsole.WriteLine(txt);
        
        Assert.True(true, txt);
    }
}