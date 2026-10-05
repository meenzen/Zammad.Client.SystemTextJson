using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class HistoryTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();
    private static Organization? Organization { get; set; }
    private static User? Customer { get; set; }
    private static Ticket? Ticket { get; set; }

    [Test]
    public async Task CreateRecords()
    {
        var client = await zammadStack.GetClientAsync();

        Organization = await client.CreateOrganizationAsync(new Organization { Name = $"HistoryTests {Id}" });
        Customer = await client.CreateUserAsync(
            new User
            {
                FirstName = "History",
                LastName = $"Customer {Id}",
                Email = $"history.{Id}@example.org",
                OrganizationId = Organization.Id,
            }
        );
        Ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = $"HistoryTests {Id}",
                GroupId = new GroupId(1),
                CustomerId = Customer.Id,
            },
            new TicketArticle
            {
                Subject = "HistoryTests",
                Body = "HistoryTests",
                Type = "note",
            }
        );

        await client.UpdateTicketTitleAsync(Ticket.Id, $"HistoryTests {Id} renamed");
        await client.UpdateUserAsync(Customer.Id, new User { FirstName = "Renamed" });
        await client.UpdateOrganizationAsync(Organization.Id, new Organization { Note = "changed" });
    }

    [Test]
    [DependsOn(nameof(CreateRecords))]
    public async Task GetTicketHistory()
    {
        var client = await zammadStack.GetClientAsync();

        // Zammad writes the history in the same transaction as the change, so it is complete right away
        var history = await client.GetTicketHistoryAsync(Ticket!.Id);

        await Assert.That(history).IsNotNull();
        var entries = history!.Entries;
        var created = entries.Find(e => e.Type == "created" && e.Object == "Ticket");
        await Assert.That(created).IsNotNull();
        await Assert.That(created!.ObjectId.Value).IsEqualTo(Ticket.Id.Value);
        var admin = await client.GetUserMeAsync();
        await Assert.That(created.CreatedById).IsEqualTo(admin.Id);

        var title = entries.Find(e => e.Type == "updated" && e.Attribute == "title");
        await Assert.That(title).IsNotNull();
        await Assert.That(title!.ValueFrom).IsEqualTo($"HistoryTests {Id}");
        await Assert.That(title.ValueTo).IsEqualTo($"HistoryTests {Id} renamed");
        await Assert.That(title.IdFrom).IsNull();

        // The article is part of the ticket's history, related to the ticket
        var article = entries.Find(e => e.Type == "created" && e.Object == "Ticket::Article");
        await Assert.That(article).IsNotNull();
        await Assert.That(article!.RelatedObject).IsEqualTo("Ticket");
        await Assert.That(article.RelatedObjectId?.Value).IsEqualTo(Ticket.Id.Value);

        // The assets contain the ticket and the users who made the changes
        await Assert.That(history.Assets.GetTicket(Ticket.Id)).IsNotNull();
        await Assert.That(history.Assets.GetUser(created.CreatedById!.Value)).IsNotNull();
    }

    [Test]
    [DependsOn(nameof(CreateRecords))]
    public async Task GetUserHistory()
    {
        var client = await zammadStack.GetClientAsync();

        var history = await client.GetUserHistoryAsync(Customer!.Id);

        await Assert.That(history).IsNotNull();
        await Assert.That(history!.Entries).Contains(e => e.Type == "created" && e.Object == "User");
        var firstname = history.Entries.Find(e => e.Type == "updated" && e.Attribute == "firstname");
        await Assert.That(firstname).IsNotNull();
        await Assert.That(firstname!.ValueFrom).IsEqualTo("History");
        await Assert.That(firstname.ValueTo).IsEqualTo("Renamed");
    }

    [Test]
    [DependsOn(nameof(CreateRecords))]
    public async Task GetOrganizationHistory()
    {
        var client = await zammadStack.GetClientAsync();

        var history = await client.GetOrganizationHistoryAsync(Organization!.Id);

        await Assert.That(history).IsNotNull();
        await Assert.That(history!.Entries).Contains(e => e.Type == "created" && e.Object == "Organization");
        var note = history.Entries.Find(e => e.Type == "updated" && e.Attribute == "note");
        await Assert.That(note).IsNotNull();
        await Assert.That(note!.ValueTo).IsEqualTo("changed");
        await Assert.That(history.Assets.GetOrganization(Organization.Id)).IsNotNull();
    }

    [Test]
    [DependsOn(nameof(GetTicketHistory))]
    [DependsOn(nameof(GetUserHistory))]
    [DependsOn(nameof(GetOrganizationHistory))]
    public async Task DeleteRecords()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTicketAsync(Ticket!.Id);
        await client.DeleteUserAsync(Customer!.Id);
        await client.DeleteOrganizationAsync(Organization!.Id);
    }
}
