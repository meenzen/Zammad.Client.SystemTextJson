using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    [InlineData(typeof(Ticket), "ticket1.json")]
    [InlineData(typeof(Ticket), "ticketExtended.json")]
    [InlineData(typeof(List<Ticket>), "tickets.json")]
    public async Task CanDeserialize(Type type, string fileName)
    {
        var json = await TestFile.ReadStringAsync("Responses", fileName);
        var result = JsonSerializer.Deserialize(json, type);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CanDeserializeExtensionData()
    {
        var json = await TestFile.ReadStringAsync("Responses", "ticketExtended.json");
        var result = JsonSerializer.Deserialize<Ticket>(json);
        Assert.NotNull(result);
        Assert.NotNull(result.ExtensionData);
        Assert.Equal(3, result.ExtensionData.Count);
        Assert.True(result.ExtensionData.ContainsKey("category"));
        Assert.True(result.ExtensionData.ContainsKey("supportclaim"));
        Assert.True(result.ExtensionData.ContainsKey("product_line"));
        Assert.Equal("", result.ExtensionData["category"].GetString());
        Assert.False(result.ExtensionData["supportclaim"].GetBoolean());
        Assert.Equal(JsonValueKind.Array, result.ExtensionData["product_line"].ValueKind);
        Assert.Single(result.ExtensionData["product_line"].EnumerateArray());
        Assert.Equal("TEST", result.ExtensionData["product_line"].EnumerateArray().First().GetString());
    }
}
