using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TicketTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();
    private static readonly string TicketTitle = $"TicketTests {Id}";
    private static TicketId? CreatedTicketId { get; set; }

    [Test]
    public async Task CreateTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = TicketTitle,
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
                OwnerId = new UserId(1),
            },
            new TicketArticle
            {
                Subject = "Help me!!!" + Id,
                Body = "Nothing Work!" + Id,
                Type = "note",
            }
        );

        await Assert.That(ticket).IsNotNull();
        await Assert.That(ticket.Id).IsNotEqualTo(TicketId.Empty);
        await Assert.That(ticket.Title).IsEqualTo(TicketTitle);
        CreatedTicketId = ticket.Id;
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    public async Task ListTickets()
    {
        var client = await zammadStack.GetClientAsync();

        var tickets = await client.ListTicketsAsync();
        await Assert.That(tickets).IsNotNull();
        await Assert.That(tickets).IsNotEmpty();
        await Assert.That(tickets).Contains(t => t.Id == CreatedTicketId);
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    public async Task ListTickets_Pagination()
    {
        var client = await zammadStack.GetClientAsync();

        var tickets = await client.ListTicketsAsync(new Pagination { Page = 1, PerPage = 20 });
        await Assert.That(tickets).IsNotNull();
        await Assert.That(tickets).IsNotEmpty();
        await Assert.That(tickets).Contains(t => t.Id == CreatedTicketId);
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    [Obsolete("Testing legacy pagination.")]
    public async Task ListTickets_Pagination_Legacy()
    {
        var client = await zammadStack.GetClientAsync();

        var tickets = await client.ListTicketsAsync(1, 20);
        await Assert.That(tickets).IsNotNull();
        await Assert.That(tickets).IsNotEmpty();
        await Assert.That(tickets).Contains(t => t.Id == CreatedTicketId);
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    [Obsolete("Testing legacy search.")]
    public async Task SearchTickets_Legacy_Sort(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var ticketSearch = await client.SearchTicketsAsync(TicketTitle, 20, "created_at", "desc");

        await Assert.That(ticketSearch).IsNotNull();
        await Assert.That(ticketSearch).IsNotEmpty();
        await Assert.That(ticketSearch).Contains(t => t.Title == TicketTitle);
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    [Obsolete("Testing legacy search.")]
    public async Task SearchTickets_Legacy(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var ticketSearch = await client.SearchTicketsAsync(TicketTitle, 20);

        await Assert.That(ticketSearch).IsNotNull();
        await Assert.That(ticketSearch).IsNotEmpty();
        await Assert.That(ticketSearch).Contains(t => t.Title == TicketTitle);
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchTickets_Pagination(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var ticketSearch = await client.SearchTicketsAsync(
            new SearchQuery
            {
                Query = TicketTitle,
                Pagination = new Pagination { PerPage = 20, Page = 1 },
                Sorting = new Sorting { SortBy = "created_at", OrderBy = OrderDirection.Descending },
            }
        );

        await Assert.That(ticketSearch).IsNotNull();
        await Assert.That(ticketSearch).IsNotEmpty();
        await Assert.That(ticketSearch).Contains(t => t.Title == TicketTitle);
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    public async Task GetTicket()
    {
        await Assert.That(CreatedTicketId).IsNotNull();
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.GetTicketAsync(CreatedTicketId.Value);

        await Assert.That(ticket).IsNotNull();
        await Assert.That(ticket.Title).IsEqualTo(TicketTitle);
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    [DependsOn(nameof(ListTickets))]
    [DependsOn(nameof(ListTickets_Pagination))]
    [DependsOn(nameof(ListTickets_Pagination_Legacy))]
    [DependsOn(nameof(SearchTickets_Legacy_Sort))]
    [DependsOn(nameof(SearchTickets_Legacy))]
    [DependsOn(nameof(SearchTickets_Pagination))]
    [DependsOn(nameof(GetTicket))]
    public async Task UpdateTicket()
    {
        await Assert.That(CreatedTicketId).IsNotNull();
        var client = await zammadStack.GetClientAsync();

        var updatedTitle = TicketTitle + " Updated";
        var updatedTicket = await client.UpdateTicketAsync(CreatedTicketId.Value, new Ticket { Title = updatedTitle });

        await Assert.That(updatedTicket).IsNotNull();
        await Assert.That(updatedTicket.Title).IsEqualTo(updatedTitle);
    }

    [Test]
    [DependsOn(nameof(GetTicket))]
    [DependsOn(nameof(UpdateTicket))]
    public async Task DeleteTicket()
    {
        await Assert.That(CreatedTicketId).IsNotNull();
        var client = await zammadStack.GetClientAsync();

        var deleteResult = await client.DeleteTicketAsync(CreatedTicketId.Value);
        await Assert.That(deleteResult).IsTrue();

        var deletedTicket = await client.GetTicketAsync(CreatedTicketId.Value);
        await Assert.That(deletedTicket).IsNull();
    }
}
