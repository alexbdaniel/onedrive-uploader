using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Application;
using Application.Graph;
using Application.Graph.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Models;
using Xunit;

namespace Application.Tests;

[TestSubject(typeof(Uploader))]
public class UploaderTest
{

    [Fact]
    public async Task UploaderDoesNotThrowException()
    {
        await using var provider = new TestServiceProvider().GetTestServiceProvider();

        var uploader = provider.GetRequiredService<Uploader>();
        
        string testFilesDirectoryName = $@"C:\Users\{Environment.UserName}\projects\TestFiles\OneDriveUploader\";
        
        List<Exception> exceptions = [];
        
        string[] filePaths = Directory.GetFiles(testFilesDirectoryName);
        foreach (string path in filePaths)
        {
            FileInfo file = new FileInfo(path);

            using var reader = new StreamReader(path);
            const string folderPath = "/TestFiles/";
            
            try
            {
                await uploader.UploadAsync(folderPath, file.Name, reader.BaseStream);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        
        Assert.Empty(exceptions);
    }
    
    
    
    
}