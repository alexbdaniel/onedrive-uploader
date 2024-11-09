using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Application.Tests;


public class TestPathModifiers
{
    private readonly ITestOutputHelper testConsole;

    public TestPathModifiers(ITestOutputHelper testConsole)
    {
        this.testConsole = testConsole;
    }

    [Fact]
    public void Thing()
    {
        const string fileName = "test123.txt";

        string sourceRelativePath = $"/LocalRelativeDirectoryPath/{fileName}";
        
        int namePosition = sourceRelativePath.LastIndexOf(fileName, StringComparison.Ordinal);
        
        
        string sourceRelativeDirectoryName = sourceRelativePath[..namePosition];
        
        const string cloudRootFolderPath = "\\/cloud-root-folder-path//";
        // string destinationDirectoryName = Path.Combine(cloudRootFolderPath, sourceRelativeDirectoryName);
        
        var builder = new StringBuilder(cloudRootFolderPath);
        builder.AppendJoin('/', sourceRelativeDirectoryName);
        builder.Replace('\\', '/');
        builder.Replace("//", "/");
        builder.Replace("//", "/");
        builder.Replace("//", "/");





        string cloudDestinationFolderPath = builder.ToString();
        
        testConsole.WriteLine(cloudDestinationFolderPath);
        
    }

    [Fact]
    public void Thing2()
    {
        const string t1 = "hello";

        string result = t1.Replace("ll", "l");
        
        testConsole.WriteLine(result);
        
        
    }
}