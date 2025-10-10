using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Zammad.Client.IntegrationTests;

public static class TestFile
{
    public static async Task<string> ReadStringAsync(string file, [CallerFilePath] string filePath = "")
    {
        var directoryPath = Path.GetDirectoryName(filePath)!;
        var fullPath = Path.Combine(directoryPath, file);
        return await File.ReadAllTextAsync(fullPath);
    }
}
