using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class SearchTests(ZammadStackFixture zammadStack)
{
    // A unique word that all records created here contain
    private static readonly string Token = TestSetup.RandomString();
    private static Organization? Organization { get; set; }
    private static User? Customer { get; set; }
    private static List<Ticket> Tickets { get; } = [];

    [Test]
    public async Task CreateRecords()
    {
        var client = await zammadStack.GetClientAsync();

        Organization = await client.CreateOrganizationAsync(new Organization { Name = $"SearchTests {Token}" });
        Customer = await client.CreateUserAsync(
            new User
            {
                FirstName = "Search",
                LastName = Token,
                Email = $"search.{Token}@example.org",
                OrganizationId = Organization.Id,
            }
        );
        for (var i = 0; i < 2; i++)
        {
            Tickets.Add(
                await client.CreateTicketAsync(
                    new Ticket
                    {
                        Title = $"SearchTests {Token}",
                        GroupId = new GroupId(1),
                        CustomerId = Customer.Id,
                    },
                    new TicketArticle
                    {
                        Subject = "SearchTests",
                        Body = "SearchTests",
                        Type = "note",
                    }
                )
            );
        }
    }

    [Test]
    [DependsOn(nameof(CreateRecords))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task Search(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var result = await client.SearchAsync(new SearchQuery { Query = Token });

        await Assert.That(result.GetTickets().ConvertAll(t => t.Id)).IsEquivalentTo(Tickets.ConvertAll(t => t.Id));
        await Assert.That(result.GetUsers()).HasSingleItem();
        await Assert.That(result.GetUsers()[0].Id).IsEqualTo(Customer!.Id);
        await Assert.That(result.GetOrganizations()).HasSingleItem();
        await Assert.That(result.GetOrganizations()[0].Id).IsEqualTo(Organization!.Id);

        // Tickets come first, then users, then organizations
        await Assert
            .That(string.Join(",", result.Hits.ConvertAll(h => h.Type)))
            .IsEqualTo("Ticket,Ticket,User,Organization");
    }

    [Test]
    [DependsOn(nameof(Search))]
    public async Task Search_Objects()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.SearchAsync(
            new SearchQuery { Query = Token },
            SearchObjectType.User,
            SearchObjectType.Organization
        );

        await Assert.That(result.Hits).All(h => h.Type is "User" or "Organization");
        await Assert.That(result.Hits.Count).IsEqualTo(2);
        await Assert.That(result.Hits).Contains(h => h.UserId == Customer!.Id);
        await Assert.That(result.Hits).Contains(h => h.OrganizationId == Organization!.Id);
    }

    [Test]
    [DependsOn(nameof(Search))]
    public async Task SearchByObject()
    {
        var client = await zammadStack.GetClientAsync();

        // The page size applies to each model
        var result = await client.SearchByObjectAsync(
            new SearchQuery
            {
                Query = Token,
                Pagination = new Pagination { PerPage = 1 },
                Sorting = new Sorting { SortBy = "id", OrderBy = OrderDirection.Ascending },
            }
        );

        var tickets = result.Get(SearchObjectType.Ticket);
        await Assert.That(tickets).IsNotNull();
        await Assert.That(tickets!.TotalCount).IsEqualTo(2);
        await Assert.That(tickets.ObjectIds).HasSingleItem();
        await Assert.That(result.GetTickets()).HasSingleItem();
        await Assert.That(result.GetTickets()[0].Id).IsEqualTo(Tickets[0].Id);

        await Assert.That(result.Get(SearchObjectType.User)?.TotalCount).IsEqualTo(1);
        await Assert.That(result.GetUsers()[0].Id).IsEqualTo(Customer!.Id);
        await Assert.That(result.GetOrganizations()[0].Id).IsEqualTo(Organization!.Id);

        var secondPage = await client.SearchByObjectAsync(
            new SearchQuery
            {
                Query = Token,
                Pagination = new Pagination { Page = 2, PerPage = 1 },
                Sorting = new Sorting { SortBy = "id", OrderBy = OrderDirection.Ascending },
            },
            SearchObjectType.Ticket
        );
        await Assert.That(secondPage.Results.Keys).IsEquivalentTo(["Ticket"]);
        await Assert.That(secondPage.GetTickets()).HasSingleItem();
        await Assert.That(secondPage.GetTickets()[0].Id).IsEqualTo(Tickets[1].Id);
    }

    [Test]
    [DependsOn(nameof(Search_Objects))]
    [DependsOn(nameof(SearchByObject))]
    public async Task DeleteRecords()
    {
        var client = await zammadStack.GetClientAsync();

        foreach (var ticket in Tickets)
        {
            await client.DeleteTicketAsync(ticket.Id);
        }

        await client.DeleteUserAsync(Customer!.Id);
        await client.DeleteOrganizationAsync(Organization!.Id);
    }
}
