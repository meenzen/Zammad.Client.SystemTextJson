using System.Net;
using TUnit.Assertions.Enums;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class ChecklistTests(ZammadStackFixture zammadStack)
{
    private const int MissingId = int.MaxValue;
    private static readonly string Id = TestSetup.RandomString();
    private static readonly string TemplateName = $"ChecklistTests {Id}";

    private static ChecklistTemplateId TemplateId { get; set; } = ChecklistTemplateId.Empty;
    private static TicketId TicketId { get; set; } = TicketId.Empty;
    private static TicketId OtherTicketId { get; set; } = TicketId.Empty;
    private static ChecklistId ChecklistId { get; set; } = ChecklistId.Empty;
    private static ChecklistItemId ItemId { get; set; } = ChecklistItemId.Empty;

    private static async Task<Ticket> CreateTicketAsync(IZammadClient client, string name) =>
        await client.CreateTicketAsync(
            new Ticket
            {
                Title = $"ChecklistTests {name} {Id}",
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
            },
            new TicketArticle
            {
                Subject = "ChecklistTests",
                Body = "ChecklistTests",
                Type = "note",
            }
        );

    [Test]
    public async Task CreateChecklistTemplate()
    {
        var client = await zammadStack.GetClientAsync();

        var template = await client.CreateChecklistTemplateAsync(
            new ChecklistTemplate
            {
                Name = TemplateName,
                Active = true,
                Items = ["First", "  Second  ", "", "Third"],
            }
        );
        TemplateId = template.Id;

        await Assert.That(template.Id).IsNotEqualTo(ChecklistTemplateId.Empty);
        await Assert.That(template.Name).IsEqualTo(TemplateName);
        await Assert.That(template.Active).IsTrue();
        // Zammad trims the texts and drops blank ones
        await Assert.That(template.Items).IsEquivalentTo(["First", "Second", "Third"], CollectionOrdering.Matching);
        await Assert.That(template.SortedItemIds!.Count).IsEqualTo(3);
    }

    [Test]
    [DependsOn(nameof(CreateChecklistTemplate))]
    public async Task ListChecklistTemplates()
    {
        var client = await zammadStack.GetClientAsync();

        var templates = await client.ListChecklistTemplatesAsync();

        var template = templates.Find(t => t.Id == TemplateId);
        await Assert.That(template).IsNotNull();
        await Assert.That(template!.Items).IsEquivalentTo(["First", "Second", "Third"], CollectionOrdering.Matching);
    }

    [Test]
    [DependsOn(nameof(ListChecklistTemplates))]
    public async Task UpdateChecklistTemplate()
    {
        var client = await zammadStack.GetClientAsync();

        var template = await client.UpdateChecklistTemplateAsync(
            TemplateId,
            new ChecklistTemplate { Name = TemplateName + " Updated", Items = ["Alpha", "Beta"] }
        );
        await Assert.That(template.Name).IsEqualTo(TemplateName + " Updated");
        await Assert.That(template.Items).IsEquivalentTo(["Alpha", "Beta"], CollectionOrdering.Matching);

        // Without items, the items are kept
        template = await client.UpdateChecklistTemplateAsync(TemplateId, new ChecklistTemplate { Active = false });
        await Assert.That(template.Active).IsFalse();
        await Assert.That(template.Name).IsEqualTo(TemplateName + " Updated");
        await Assert.That(template.Items).IsEquivalentTo(["Alpha", "Beta"], CollectionOrdering.Matching);
    }

    [Test]
    [DependsOn(nameof(UpdateChecklistTemplate))]
    public async Task CreateChecklistFromTemplate()
    {
        var client = await zammadStack.GetClientAsync();
        TicketId = (await CreateTicketAsync(client, "Template")).Id;

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateChecklistFromTemplateAsync(TicketId, TemplateId)
        );
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert
            .That(exception.Error)
            .IsEqualTo("Checklist template must be active to use as a checklist starting point.");

        await client.UpdateChecklistTemplateAsync(TemplateId, new ChecklistTemplate { Active = true });
        var checklist = await client.CreateChecklistFromTemplateAsync(TicketId, TemplateId);
        ChecklistId = checklist.Id;

        await Assert.That(checklist.Name).IsEqualTo(TemplateName + " Updated");
        await Assert
            .That(checklist.Items!.Select(i => i.Text ?? ""))
            .IsEquivalentTo(["Alpha", "Beta"], CollectionOrdering.Matching);
        await Assert.That(checklist.Items!).All(i => i.Checked == false && i.ChecklistId == checklist.Id);
        await Assert.That((await client.GetTicketAsync(TicketId))!.ChecklistId).IsEqualTo(checklist.Id);
    }

    [Test]
    [DependsOn(nameof(CreateChecklistFromTemplate))]
    public async Task GetChecklist()
    {
        var client = await zammadStack.GetClientAsync();

        var checklist = await client.GetChecklistAsync(ChecklistId);

        await Assert.That(checklist).IsNotNull();
        await Assert
            .That(checklist!.Items!.Select(i => i.Text ?? ""))
            .IsEquivalentTo(["Alpha", "Beta"], CollectionOrdering.Matching);
        await Assert.That(checklist.ItemIds!.Count).IsEqualTo(2);
        await Assert
            .That(checklist.SortedItemIds)
            .IsEquivalentTo(checklist.Items!.Select(i => i.Id).ToList(), CollectionOrdering.Matching);
    }

    [Test]
    [DependsOn(nameof(GetChecklist))]
    public async Task CreateChecklistItems()
    {
        var client = await zammadStack.GetClientAsync();

        var item = await client.CreateChecklistItemAsync(ChecklistId, new ChecklistItem { Text = "Gamma" });
        ItemId = item.Id;
        await Assert.That(item.Text).IsEqualTo("Gamma");
        await Assert.That(item.Checked).IsFalse();
        await Assert.That(item.ChecklistId).IsEqualTo(ChecklistId);

        var ids = await client.CreateChecklistItemsAsync(
            ChecklistId,
            [new ChecklistItem { Text = "Delta" }, new ChecklistItem { Text = "Epsilon", Checked = true }]
        );
        await Assert.That(ids.Count).IsEqualTo(2);

        // New items are added to the end
        var checklist = await client.GetChecklistAsync(ChecklistId);
        await Assert
            .That(checklist!.Items!.Select(i => i.Text ?? ""))
            .IsEquivalentTo(["Alpha", "Beta", "Gamma", "Delta", "Epsilon"], CollectionOrdering.Matching);
        await Assert.That(checklist.Items!.Skip(3).Select(i => i.Id)).IsEquivalentTo(ids, CollectionOrdering.Matching);
        await Assert.That(checklist.Items![4].Checked).IsTrue();
    }

    [Test]
    [DependsOn(nameof(CreateChecklistItems))]
    public async Task UpdateChecklistItem()
    {
        var client = await zammadStack.GetClientAsync();

        var item = await client.UpdateChecklistItemAsync(ItemId, new ChecklistItem { Checked = true });
        await Assert.That(item.Checked).IsTrue();
        await Assert.That(item.Text).IsEqualTo("Gamma");

        item = await client.UpdateChecklistItemAsync(ItemId, new ChecklistItem { Text = "Gamma Updated" });
        await Assert.That(item.Text).IsEqualTo("Gamma Updated");
        await Assert.That(item.Checked).IsTrue();

        var fetched = await client.GetChecklistItemAsync(ItemId);
        await Assert.That(fetched).IsNotNull();
        await Assert.That(fetched!.Text).IsEqualTo("Gamma Updated");
    }

    [Test]
    [DependsOn(nameof(UpdateChecklistItem))]
    public async Task UpdateChecklist_Reorder()
    {
        var client = await zammadStack.GetClientAsync();
        var checklist = await client.GetChecklistAsync(ChecklistId);
        var reversed = checklist!.SortedItemIds!.AsEnumerable().Reverse().ToList();

        var updated = await client.UpdateChecklistAsync(
            ChecklistId,
            new Checklist { Name = "Renamed " + Id, SortedItemIds = reversed }
        );

        await Assert.That(updated.Name).IsEqualTo("Renamed " + Id);
        await Assert.That(updated.SortedItemIds).IsEquivalentTo(reversed, CollectionOrdering.Matching);
        await Assert
            .That(updated.Items!.Select(i => i.Text ?? ""))
            .IsEquivalentTo(["Epsilon", "Delta", "Gamma Updated", "Beta", "Alpha"], CollectionOrdering.Matching);

        // null keeps the current values
        updated = await client.UpdateChecklistAsync(ChecklistId, new Checklist());
        await Assert.That(updated.Name).IsEqualTo("Renamed " + Id);
        await Assert.That(updated.SortedItemIds).IsEquivalentTo(reversed, CollectionOrdering.Matching);
    }

    [Test]
    [DependsOn(nameof(UpdateChecklist_Reorder))]
    public async Task DeleteChecklistItem()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteChecklistItemAsync(ItemId);

        var checklist = await client.GetChecklistAsync(ChecklistId);
        await Assert.That(checklist!.SortedItemIds).DoesNotContain(ItemId);
        await Assert
            .That(checklist.Items!.Select(i => i.Text ?? ""))
            .IsEquivalentTo(["Epsilon", "Delta", "Beta", "Alpha"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task CreateTicketChecklistItems()
    {
        var client = await zammadStack.GetClientAsync();
        OtherTicketId = (await CreateTicketAsync(client, "Items")).Id;

        // Creates the checklist on demand
        var item = await client.CreateTicketChecklistItemAsync(OtherTicketId, new ChecklistItem { Text = "One" });
        var ticket = await client.GetTicketAsync(OtherTicketId);
        await Assert.That(ticket!.ChecklistId).IsEqualTo(item.ChecklistId);

        var ids = await client.CreateTicketChecklistItemsAsync(
            OtherTicketId,
            [new ChecklistItem { Text = "Two" }, new ChecklistItem { Text = "Three" }]
        );

        var checklist = await client.GetChecklistAsync(item.ChecklistId!.Value);
        await Assert.That(checklist!.Name).IsEqualTo("");
        await Assert
            .That(checklist.Items!.Select(i => i.Text ?? ""))
            .IsEquivalentTo(["One", "Two", "Three"], CollectionOrdering.Matching);
        await Assert.That(checklist.Items!.Skip(1).Select(i => i.Id)).IsEquivalentTo(ids, CollectionOrdering.Matching);
    }

    [Test]
    [DependsOn(nameof(CreateTicketChecklistItems))]
    [DependsOn(nameof(CreateChecklistFromTemplate))]
    public async Task CreateChecklistItem_ReferencingTicket()
    {
        var client = await zammadStack.GetClientAsync();
        var referenced = await client.GetTicketAsync(TicketId);

        // Zammad recognizes ticket numbers with the ticket hook, but not bare numbers
        var item = await client.CreateTicketChecklistItemAsync(
            OtherTicketId,
            new ChecklistItem { Text = "Ticket#" + referenced!.Number }
        );
        await Assert.That(item.TicketId).IsEqualTo(TicketId);
        await Assert.That(item.Checked).IsFalse();

        var bare = await client.CreateTicketChecklistItemAsync(
            OtherTicketId,
            new ChecklistItem { Text = referenced.Number }
        );
        await Assert.That(bare.TicketId).IsNull();
    }

    [Test]
    [DependsOn(nameof(CreateChecklistItem_ReferencingTicket))]
    public async Task CreateChecklist_WithFirstItem()
    {
        var client = await zammadStack.GetClientAsync();
        var oldChecklistId = (await client.GetTicketAsync(OtherTicketId))!.ChecklistId!.Value;

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateChecklistAsync(OtherTicketId, createFirstItem: true)
        );
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("This ticket already has a checklist.");

        await client.DeleteChecklistAsync(oldChecklistId);
        var checklist = await client.CreateChecklistAsync(OtherTicketId, createFirstItem: true);

        await Assert.That(checklist.Id).IsNotEqualTo(oldChecklistId);
        await Assert.That(checklist.Items).HasSingleItem();
        await Assert.That(checklist.Items![0].Text).IsEqualTo("");
        await Assert.That((await client.GetTicketAsync(OtherTicketId))!.ChecklistId).IsEqualTo(checklist.Id);
    }

    [Test]
    public async Task GetMissing_IsForbidden()
    {
        var client = await zammadStack.GetClientAsync();

        var checklist = await Assert.ThrowsAsync<ZammadException>(() =>
            client.GetChecklistAsync(new ChecklistId(MissingId))
        );
        await Assert.That(checklist!.Code).IsEqualTo(HttpStatusCode.Forbidden);

        var item = await Assert.ThrowsAsync<ZammadException>(() =>
            client.GetChecklistItemAsync(new ChecklistItemId(MissingId))
        );
        await Assert.That(item!.Code).IsEqualTo(HttpStatusCode.Forbidden);

        await Assert.That(await client.GetChecklistTemplateAsync(new ChecklistTemplateId(MissingId))).IsNull();
    }

    [Test]
    [DependsOn(nameof(DeleteChecklistItem))]
    public async Task DeleteChecklist()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteChecklistAsync(ChecklistId);

        await Assert.That((await client.GetTicketAsync(TicketId))!.ChecklistId).IsNull();
        var exception = await Assert.ThrowsAsync<ZammadException>(() => client.GetChecklistAsync(ChecklistId));
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    [DependsOn(nameof(DeleteChecklist))]
    public async Task DeleteChecklistTemplate()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteChecklistTemplateAsync(TemplateId);

        await Assert.That(await client.GetChecklistTemplateAsync(TemplateId)).IsNull();
    }

    [Test]
    [DependsOn(nameof(DeleteChecklist))]
    [DependsOn(nameof(CreateChecklist_WithFirstItem))]
    public async Task DeleteTickets()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTicketAsync(TicketId);
        await client.DeleteTicketAsync(OtherTicketId);
    }
}
