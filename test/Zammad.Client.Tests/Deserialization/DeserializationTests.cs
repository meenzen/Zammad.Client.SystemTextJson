using System.Runtime.CompilerServices;
using System.Text.Json;
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
    [Test]
    [Arguments(typeof(Ticket), "ticket.json")]
    [Arguments(typeof(Ticket), "ticket1.json")]
    [Arguments(typeof(Ticket), "ticketExtended.json")]
    [Arguments(typeof(List<Ticket>), "tickets.json")]
    [Arguments(typeof(TicketAccounting), "ticketAccounting.json")]
    [Arguments(typeof(List<TicketAccounting>), "ticketAccountings.json")]
    public async Task CanDeserialize(Type type, string fileName)
    {
        var json = await TestFile.ReadStringAsync("Responses", fileName);
        var result = JsonSerializer.Deserialize(json, type);
        await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task CanDeserializeExtensionData()
    {
        var json = await TestFile.ReadStringAsync("Responses", "ticketExtended.json");
        var result = JsonSerializer.Deserialize<Ticket>(json);
        await Assert.That(result).IsNotNull();
        await Assert.That(result.ExtensionData).IsNotNull();
        await Assert.That(result.ExtensionData.Count).IsEqualTo(3);
        await Assert.That(result.ExtensionData.ContainsKey("category")).IsTrue();
        await Assert.That(result.ExtensionData.ContainsKey("supportclaim")).IsTrue();
        await Assert.That(result.ExtensionData.ContainsKey("product_line")).IsTrue();
        await Assert.That(result.ExtensionData["category"].GetString()).IsEqualTo("");
        await Assert.That(result.ExtensionData["supportclaim"].GetBoolean()).IsFalse();
        await Assert.That(result.ExtensionData["product_line"].ValueKind).IsEqualTo(JsonValueKind.Array);
        await Assert.That(result.ExtensionData["product_line"].EnumerateArray()).HasSingleItem();
        await Assert.That(result.ExtensionData["product_line"].EnumerateArray().First().GetString()).IsEqualTo("TEST");
    }
}
