using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Zammad.Client.Resources;

namespace Zammad.Client.Tests.Deserialization;

public static class TestFile
{
    public static async Task<string> ReadStringAsync(
        string directory,
        string file,
        [CallerFilePath] string filePath = ""
    )
    {
        var directoryPath = Path.GetDirectoryName(filePath)!;
        var fullPath = Path.Combine(directoryPath, directory, file);
        return await File.ReadAllTextAsync(fullPath);
    }
}

public class DeserializationTests
{
    [Theory]
    [InlineData(typeof(Ticket), "ticket.json")]
    [InlineData(typeof(List<Ticket>), "tickets.json")]
    public async Task CanDeserialize(Type type, string fileName)
    {
        var json = await TestFile.ReadStringAsync("Responses", fileName);
        var result = JsonSerializer.Deserialize(json, type);
        Assert.NotNull(result);
    }
}
