using System;
using System.IO;
using System.Net;

namespace Application.Tests;

internal static class TestUtilities
{
    internal static bool IsSuccessful(this HttpStatusCode statusCode) =>
        (int)statusCode >= 200 && (int)statusCode <= 299;


    /// <summary>
    /// Creates temporary file and fills it with content
    /// </summary>
    /// <param name="fullPath">A target path for this file.</param>
    /// <param name="size">Target size in megabytes.</param>
    /// <returns>Full path to the temporary file.</returns>
    internal static FileInfo CreateTemporaryFileWithContent(string fullPath, double size = 0.5)
    {
        //https://stackoverflow.com/a/4432207
        const int blockSize = 1024 * 8;
        const int blocksPerMb = (1024 * 1024) / blockSize;
        byte[] data = new byte[blockSize];
        Random rng = new Random();
        using FileStream stream = File.OpenWrite(fullPath);
        // There 
        for (int i = 0; i < size * blocksPerMb; i++)
        {
            rng.NextBytes(data);
            stream.Write(data, 0, data.Length);
        }

        var file = new FileInfo(fullPath);
        
        return file;
    }

    /// <summary>
    /// Creates temporary file and fills it with content
    /// </summary>
    /// <param name="size">Target size in megabytes.</param>
    /// <returns>Full path to the temporary file.</returns>
    internal static FileInfo CreateTemporaryFileWithContent(double size = 0.5)
    {
        string fullPath = Path.GetTempFileName();
        
        //https://stackoverflow.com/a/4432207
        const int blockSize = 1024 * 8;
        const int blocksPerMb = (1024 * 1024) / blockSize;
        byte[] data = new byte[blockSize];
        Random rng = new Random();
        using FileStream stream = File.OpenWrite(fullPath);
        // There 
        for (int i = 0; i < size * blocksPerMb; i++)
        {
            rng.NextBytes(data);
            stream.Write(data, 0, data.Length);
        }

        var file = new FileInfo(fullPath);
        
        return file;
    }
    
    
    
}