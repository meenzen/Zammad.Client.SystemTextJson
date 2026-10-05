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
    [Arguments(typeof(List<TicketAccounting>), "timeAccountings.json")]
    [Arguments(typeof(List<TimeAccountingType>), "timeAccountingTypes.json")]
    [Arguments(typeof(List<TimeAccountingActivityRow>), "timeAccountingByActivity.json")]
    [Arguments(typeof(List<TimeAccountingTicketRow>), "timeAccountingByTicket.json")]
    [Arguments(typeof(List<TimeAccountingCustomerRow>), "timeAccountingByCustomer.json")]
    [Arguments(typeof(List<TimeAccountingOrganizationRow>), "timeAccountingByOrganization.json")]
    [Arguments(typeof(Role), "role.json")]
    [Arguments(typeof(Role), "roleExpanded.json")]
    [Arguments(typeof(List<Role>), "roles.json")]
    [Arguments(typeof(List<Role>), "rolesSearch.json")]
    [Arguments(typeof(UserAccessTokenList), "userAccessTokens.json")]
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
    [Arguments(typeof(Macro), "macro.json")]
    [Arguments(typeof(Macro), "macroExpanded.json")]
    [Arguments(typeof(List<Macro>), "macros.json")]
    [Arguments(typeof(List<Macro>), "macrosSearch.json")]
    [Arguments(typeof(Overview), "overview.json")]
    [Arguments(typeof(Overview), "overviewExpanded.json")]
    [Arguments(typeof(List<Overview>), "overviews.json")]
    [Arguments(typeof(List<Overview>), "overviewsSearch.json")]
    [Arguments(typeof(Template), "template.json")]
    [Arguments(typeof(Template), "templateExpanded.json")]
    [Arguments(typeof(List<Template>), "templates.json")]
    [Arguments(typeof(List<Template>), "templatesSearch.json")]
    [Arguments(typeof(TextModule), "textModule.json")]
    [Arguments(typeof(TextModule), "textModuleExpanded.json")]
    [Arguments(typeof(List<TextModule>), "textModules.json")]
    [Arguments(typeof(List<TextModule>), "textModulesSearch.json")]
    [Arguments(typeof(Signature), "signature.json")]
    [Arguments(typeof(Signature), "signatureExpanded.json")]
    [Arguments(typeof(List<Signature>), "signatures.json")]
    [Arguments(typeof(Trigger), "trigger.json")]
    [Arguments(typeof(List<Trigger>), "triggers.json")]
    [Arguments(typeof(Job), "job.json")]
    [Arguments(typeof(List<Job>), "jobs.json")]
    [Arguments(typeof(Webhook), "webhook.json")]
    [Arguments(typeof(List<Webhook>), "webhooks.json")]
    [Arguments(typeof(List<PreDefinedWebhook>), "webhooksPreDefined.json")]
    [Arguments(typeof(Dictionary<string, List<string>>), "webhookReplacements.json")]
    [Arguments(typeof(CoreWorkflow), "coreWorkflow.json")]
    [Arguments(typeof(List<CoreWorkflow>), "coreWorkflows.json")]
    [Arguments(typeof(Calendar), "calendar.json")]
    [Arguments(typeof(List<Calendar>), "calendars.json")]
    [Arguments(typeof(Zammad.Client.Resources.Internal.CalendarTimezones), "calendarTimezones.json")]
    [Arguments(typeof(Sla), "sla.json")]
    [Arguments(typeof(List<Sla>), "slas.json")]
    [Arguments(typeof(PostmasterFilter), "postmasterFilter.json")]
    [Arguments(typeof(List<PostmasterFilter>), "postmasterFilters.json")]
    [Arguments(typeof(Assets), "assets.json")]
    [Arguments(typeof(LinkList), "links.json")]
    [Arguments(typeof(MentionList), "mentions.json")]
    [Arguments(typeof(Checklist), "checklist.json")]
    [Arguments(typeof(FullResponse), "checklistFull.json")]
    [Arguments(typeof(FullResponse), "checklistCreate.json")]
    [Arguments(typeof(ChecklistItem), "checklistItem.json")]
    [Arguments(typeof(ChecklistItemBulkResponse), "checklistItemsBulk.json")]
    [Arguments(typeof(ChecklistTemplate), "checklistTemplate.json")]
    [Arguments(typeof(List<ChecklistTemplate>), "checklistTemplates.json")]
    [Arguments(typeof(FullResponse), "checklistTemplatesFull.json")]
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
