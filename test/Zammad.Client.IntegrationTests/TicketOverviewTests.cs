using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TicketOverviewTests(ZammadStackFixture zammadStack)
{
    private const string UnassignedLink = "all_unassigned";
    private static readonly string Id = TestSetup.RandomString();
    private static Ticket? Ticket { get; set; }

    [Test]
    public async Task CreateTicket()
    {
        var client = await zammadStack.GetClientAsync();

        // New, without owner: shows up in "Unassigned & Open Tickets"
        Ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = $"TicketOverviewTests {Id}",
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
            },
            new TicketArticle
            {
                Subject = "TicketOverviewTests",
                Body = "TicketOverviewTests",
                Type = "note",
            }
        );
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    public async Task ListTicketOverviews()
    {
        var client = await zammadStack.GetClientAsync();

        var overviews = await client.ListTicketOverviewsAsync();

        var unassigned = overviews.Find(o => o.Link == UnassignedLink);
        await Assert.That(unassigned).IsNotNull();
        await Assert.That(unassigned!.Name).IsEqualTo("Unassigned & Open Tickets");
        await Assert.That(unassigned.Count).IsGreaterThanOrEqualTo(1);
        var prios = overviews.ConvertAll(o => o.Prio ?? 0);
        await Assert.That(string.Join(",", prios)).IsEqualTo(string.Join(",", prios.OrderBy(p => p)));
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    public async Task GetTicketOverview()
    {
        var client = await zammadStack.GetClientAsync();

        var overview = await client.GetTicketOverviewAsync(UnassignedLink);

        await Assert.That(overview).IsNotNull();
        await Assert.That(overview!.Index.Overview!.View).IsEqualTo(UnassignedLink);
        await Assert.That(overview.Index.Count).IsGreaterThanOrEqualTo(overview.Index.Tickets.Count);
        await Assert.That(overview.Index.Tickets).Contains(t => t.Id == Ticket!.Id);
        await Assert.That(overview.GetTickets()).Contains(t => t.Title == Ticket!.Title);
        await Assert.That(overview.Assets.GetOverview(overview.Index.Overview.Id)?.Link).IsEqualTo(UnassignedLink);
    }

    [Test]
    public async Task GetTicketOverview_Unknown()
    {
        var client = await zammadStack.GetClientAsync();

        await Assert.That(await client.GetTicketOverviewAsync($"does_not_exist_{Id}")).IsNull();
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    public async Task ListTicketOverviews_Agent()
    {
        var client = await zammadStack.GetClientOnBehalfOfAsync("agent1@example.org");

        // agent1 is in the ticket's group, so it counts the ticket too
        var overview = await client.GetTicketOverviewAsync(UnassignedLink);
        await Assert.That(overview).IsNotNull();
        await Assert.That(overview!.Index.Tickets).Contains(t => t.Id == Ticket!.Id);
    }

    [Test]
    [DependsOn(nameof(ListTicketOverviews))]
    [DependsOn(nameof(GetTicketOverview))]
    [DependsOn(nameof(ListTicketOverviews_Agent))]
    public async Task DeleteTicket()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTicketAsync(Ticket!.Id);
    }
}
