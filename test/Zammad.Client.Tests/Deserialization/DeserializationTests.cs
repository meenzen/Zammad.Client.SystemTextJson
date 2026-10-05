using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;
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
    [Arguments(typeof(MonitoringStatus), "monitoringStatus.json")]
    [Arguments(typeof(AmountCheckResult), "amountCheck.json")]
    [Arguments(typeof(List<TwoFactorMethod>), "twoFactorMethods.json")]
    [Arguments(typeof(VersionResponse), "version.json")]
    [Arguments(typeof(DataPrivacyTask), "dataPrivacyTask.json")]
    [Arguments(typeof(List<DataPrivacyTask>), "dataPrivacyTasks.json")]
    [Arguments(typeof(DataPrivacyTasksByStateResponse), "dataPrivacyTasksByState.json")]
    [Arguments(typeof(Setting), "setting.json")]
    [Arguments(typeof(List<Setting>), "settings.json")]
    [Arguments(typeof(EmailAddress), "emailAddress.json")]
    [Arguments(typeof(List<EmailAddress>), "emailAddresses.json")]
    [Arguments(typeof(OnlineNotification), "notification.json")]
    [Arguments(typeof(List<OnlineNotification>), "notifications.json")]
    [Arguments(typeof(User), "userExpanded.json")]
    [Arguments(typeof(List<User>), "usersExpanded.json")]
    [Arguments(typeof(Organization), "organizationExpanded.json")]
    [Arguments(typeof(Group), "groupExpanded.json")]
    [Arguments(typeof(Ticket), "ticketZammad72.json")]
    [Arguments(typeof(Ticket), "ticketExpandedZammad72.json")]
    [Arguments(typeof(List<Ticket>), "ticketsZammad72.json")]
    [Arguments(typeof(List<Ticket>), "ticketsSearch.json")]
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

    /// <summary>
    /// <see cref="IHasCustomFields.ExtensionData"/> is only meant for custom attributes. These responses were recorded
    /// without any, so every field has to be mapped to a property.
    /// </summary>
    [Test]
    [Arguments(typeof(User), "user.json")]
    [Arguments(typeof(User), "userExpanded.json")]
    [Arguments(typeof(List<User>), "users.json")]
    [Arguments(typeof(List<User>), "usersExpanded.json")]
    [Arguments(typeof(List<User>), "usersSearch.json")]
    [Arguments(typeof(Organization), "organization.json")]
    [Arguments(typeof(Organization), "organizationExpanded.json")]
    [Arguments(typeof(List<Organization>), "organizations.json")]
    [Arguments(typeof(List<Organization>), "organizationsSearch.json")]
    [Arguments(typeof(Group), "group.json")]
    [Arguments(typeof(Group), "groupExpanded.json")]
    [Arguments(typeof(List<Group>), "groups.json")]
    [Arguments(typeof(Ticket), "ticketZammad72.json")]
    [Arguments(typeof(Ticket), "ticketExpandedZammad72.json")]
    [Arguments(typeof(List<Ticket>), "ticketsZammad72.json")]
    [Arguments(typeof(List<Ticket>), "ticketsSearch.json")]
    public async Task MapsAllBuiltInFields(Type type, string fileName)
    {
        var json = await TestFile.ReadStringAsync("Responses", fileName);
        var result = JsonSerializer.Deserialize(json, type, Serialization.GetOptions());

        IEnumerable<IHasCustomFields> items = result switch
        {
            IHasCustomFields item => [item],
            System.Collections.IEnumerable list => list.Cast<IHasCustomFields>(),
            _ => throw new InvalidOperationException($"Unexpected result type {result?.GetType()}"),
        };
        foreach (var item in items)
        {
            await Assert.That(item.ExtensionData?.Keys ?? Enumerable.Empty<string>()).IsEmpty();
        }
    }

    [Test]
    public async Task CanDeserializeGroupWithAssignmentTimeout()
    {
        var group = JsonSerializer.Deserialize<Group>(
            """{"id":1,"name":"Users","active":true,"assignment_timeout":120,"user_ids":[3,4]}""",
            Serialization.GetOptions()
        );

        await Assert.That(group).IsNotNull();
        await Assert.That(group!.AssignmentTimeout).IsEqualTo(120);
        await Assert.That(group.UserIds).IsEquivalentTo([new UserId(3), new UserId(4)]);
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
