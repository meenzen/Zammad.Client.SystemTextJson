using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Object = Zammad.Client.Resources.Object;

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
    [Arguments(typeof(List<Object>), "objects.json")]
    [Arguments(typeof(List<Object>), "objects1.json")]
    [Arguments(typeof(Object), "objectBoolean.json")]
    [Arguments(typeof(Object), "objectDate.json")]
    [Arguments(typeof(Object), "objectDateTime.json")]
    [Arguments(typeof(Object), "objectInteger.json")]
    [Arguments(typeof(Object), "objectSelect.json")]
    [Arguments(typeof(Object), "objectText.json")]
    [Arguments(typeof(Object), "objectTreeSelect.json")]
    [Arguments(typeof(HealthCheckResult), "healthCheck.json")]
    [Arguments(typeof(EmailAddress), "emailAddress.json")]
    [Arguments(typeof(List<EmailAddress>), "emailAddresses.json")]
    [Arguments(typeof(OnlineNotification), "notification.json")]
    [Arguments(typeof(List<OnlineNotification>), "notifications.json")]
    [Arguments(typeof(User), "user.json")]
    [Arguments(typeof(List<User>), "users.json")]
    [Arguments(typeof(List<User>), "usersSearch.json")]
    [Arguments(typeof(Organization), "organization.json")]
    [Arguments(typeof(List<Organization>), "organizations.json")]
    [Arguments(typeof(List<Organization>), "organizationsSearch.json")]
    [Arguments(typeof(Group), "group.json")]
    [Arguments(typeof(List<Group>), "groups.json")]
    [Arguments(typeof(TicketState), "ticketState.json")]
    [Arguments(typeof(List<TicketState>), "ticketStates.json")]
    [Arguments(typeof(TicketPriority), "ticketPriority.json")]
    [Arguments(typeof(List<TicketPriority>), "ticketPriorities.json")]
    [Arguments(typeof(TicketArticle), "ticketArticle.json")]
    [Arguments(typeof(TicketArticle), "ticketArticleWithAttachment.json")]
    [Arguments(typeof(List<TicketArticle>), "ticketArticles.json")]
    [Arguments(typeof(List<Tag>), "tagList.json")]
    [Arguments(typeof(List<TagSearchResult>), "tagSearch.json")]
    public async Task CanDeserialize(Type type, string fileName)
    {
        var options = Serialization.GetOptions();
        options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;

        var json = await TestFile.ReadStringAsync("Responses", fileName);
        var result = JsonSerializer.Deserialize(json, type, options);
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
