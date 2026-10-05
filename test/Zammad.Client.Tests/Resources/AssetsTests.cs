using System.Text.Json;
using TUnit.Assertions.Enums;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;
using Zammad.Client.Tests.Deserialization;

namespace Zammad.Client.Tests.Resources;

public class AssetsTests
{
    private static async Task<T> DeserializeAsync<T>(string fileName)
    {
        var json = await TestFile.ReadStringAsync("../Deserialization/Responses", fileName);
        return JsonSerializer.Deserialize<T>(json, Serialization.GetOptions()) ?? throw new InvalidOperationException();
    }

    [Test]
    public async Task LinkList_ContainsLinkedTicketAndUsers()
    {
        var list = await DeserializeAsync<LinkList>("links.json");

        await Assert.That(list.Links).HasSingleItem();
        var link = list.Links[0];
        await Assert.That(link.LinkType).IsEqualTo(LinkType.Parent);
        await Assert.That(link.TicketId).IsEqualTo(new TicketId(14));

        var ticket = list.Assets.GetTicket(link.TicketId!.Value);
        await Assert.That(ticket).IsNotNull();
        await Assert.That(ticket!.Number).IsEqualTo("64014");
        await Assert.That(list.Assets.GetUser(ticket.CreatedById!.Value)!.Login).IsEqualTo("admin@example.org");
        await Assert.That(list.Assets.Users.Keys).IsEquivalentTo([3, 1]);
        await Assert.That(list.Assets.ExtensionData).IsNull();
    }

    [Test]
    public async Task TicketAssets_ContainAllTypedModels()
    {
        // The assets of GET /api/v1/tickets/1?all=true (Zammad's welcome ticket)
        var assets = await DeserializeAsync<Assets>("assets.json");

        var ticket = assets.GetTicket(new TicketId(1));
        await Assert.That(ticket).IsNotNull();
        await Assert.That(ticket!.ArticleIds).IsEquivalentTo([new ArticleId(1)]);
        await Assert.That(assets.GetTicketArticle(new ArticleId(1))!.TicketId).IsEqualTo(ticket.Id);
        await Assert.That(assets.GetOrganization(ticket.OrganizationId!.Value)!.Name).IsEqualTo("Zammad Foundation");
        await Assert.That(assets.GetUser(ticket.CustomerId!.Value)!.Login).IsEqualTo("nicole.braun@zammad.org");
        await Assert.That(assets.Users.Count).IsEqualTo(4);
        await Assert.That(assets.ExtensionData).IsNull();

        IEnumerable<IHasCustomFields> records =
        [
            .. assets.Tickets.Values,
            .. assets.Users.Values,
            .. assets.Organizations.Values,
        ];
        foreach (var record in records)
        {
            await Assert.That(record.ExtensionData?.Keys ?? Enumerable.Empty<string>()).IsEmpty();
        }
    }

    [Test]
    public async Task Get_ReturnsNullForMissingRecord()
    {
        var list = await DeserializeAsync<LinkList>("links.json");

        await Assert.That(list.Assets.GetTicket(new TicketId(1))).IsNull();
        await Assert.That(list.Assets.GetOrganization(new OrganizationId(1))).IsNull();
        await Assert.That(list.Assets.Organizations).IsEmpty();
    }

    [Test]
    public async Task Records_MapAllBuiltInFields()
    {
        var list = await DeserializeAsync<LinkList>("links.json");
        var full = await DeserializeAsync<FullResponse>("checklistFull.json");

        IEnumerable<IHasCustomFields> records =
        [
            .. list.Assets.Tickets.Values,
            .. list.Assets.Users.Values,
            .. full.Assets.Tickets.Values,
            .. full.Assets.Users.Values,
        ];
        foreach (var record in records)
        {
            await Assert.That(record.ExtensionData?.Keys ?? Enumerable.Empty<string>()).IsEmpty();
        }
    }

    [Test]
    public async Task Checklist_ContainsItems()
    {
        var full = await DeserializeAsync<FullResponse>("checklistFull.json");

        var checklist = full.Assets.GetChecklist(new ChecklistId(full.Id!.Value));
        await Assert.That(checklist).IsNotNull();
        await Assert
            .That(checklist!.SortedItemIds)
            .IsEquivalentTo(
                [new ChecklistItemId(10), new ChecklistItemId(11), new ChecklistItemId(12), new ChecklistItemId(13)],
                CollectionOrdering.Matching
            );
        await Assert.That(full.Assets.GetChecklistItem(new ChecklistItemId(12))!.Text).IsEqualTo("Three");
        await Assert.That(full.Assets.Tickets[14].ChecklistId).IsEqualTo(checklist.Id);
    }

    [Test]
    public async Task ChecklistTemplates_ContainItems()
    {
        var full = await DeserializeAsync<FullResponse>("checklistTemplatesFull.json");

        await Assert.That(full.RecordIds).IsEquivalentTo([2]);
        await Assert.That(full.TotalCount).IsEqualTo(1);
        var template = full.Assets.GetChecklistTemplate(new ChecklistTemplateId(2));
        await Assert
            .That(template!.SortedItemIds)
            .IsEquivalentTo(
                [new ChecklistTemplateItemId(6), new ChecklistTemplateItemId(7)],
                CollectionOrdering.Matching
            );
        await Assert.That(full.Assets.GetChecklistTemplateItem(new ChecklistTemplateItemId(7))!.Text).IsEqualTo("Two");
    }

    [Test]
    public async Task OtherModels_GoToExtensionData()
    {
        var assets = JsonSerializer.Deserialize<Assets>(
            """
            {
              "Ticket": {},
              "Role": { "1": { "id": 1, "name": "Admin" } },
              "TicketState": { "2": { "id": 2, "name": "open" } }
            }
            """,
            Serialization.GetOptions()
        );

        await Assert.That(assets!.Tickets).IsEmpty();
        await Assert.That(assets.ExtensionData!.Keys).IsEquivalentTo(["Role", "TicketState"]);
        await Assert
            .That(assets.ExtensionData["Role"].GetProperty("1").GetProperty("name").GetString())
            .IsEqualTo("Admin");
    }
}
