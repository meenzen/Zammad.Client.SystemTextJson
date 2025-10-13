using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Zammad.Client.IntegrationTests;

public static class TestFile
{
    public static async Task<string> ReadStringAsync(string file, [CallerFilePath] string filePath = "")
    {
        var directoryPath = Path.GetDirectoryName(NormalizeCallerFilePath(filePath))!;
        var fullPath = Path.Combine(directoryPath, file);
        return await File.ReadAllTextAsync(fullPath);
    }

    public static string GetAbsolutePath(string file, [CallerFilePath] string filePath = "")
    {
        var directoryPath = Path.GetDirectoryName(NormalizeCallerFilePath(filePath))!;
        var fullPath = Path.Combine(directoryPath, file);
        return fullPath;
    }

    private static string NormalizeCallerFilePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return filePath;
        }

        // Azure Pipelines can expose source paths prefixed with "/_/" (and some other CI systems may do similar).
        // Map that to the actual checkout directory if one of the common CI environment variables is present,
        // or fall back to the current working directory.
        if (!filePath.StartsWith("/_/"))
        {
            return filePath;
        }

        var sources =
            Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY")
            ?? // Azure Pipelines
            Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")
            ?? // GitHub Actions
            Directory.GetCurrentDirectory(); // fallback

        var relative = filePath.Substring(3).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Path.Combine(sources, relative);
    }
}
