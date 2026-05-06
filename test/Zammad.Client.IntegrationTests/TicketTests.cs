using System;
using System.Threading;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TicketTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();

    [Test]
    [DependsOn<OrganizationTests>(nameof(OrganizationTests.Organization_Delete_Test))]
    public async Task Ticket_Create_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = "Help me!" + Id,
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
    }

    [Test]
    [DependsOn<TicketTests>(nameof(Ticket_Create_Test))]
    public async Task Ticket_Search_Test(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var ticketSearch = await client.SearchTicketsAsync(
            new SearchQuery
            {
                Query = Id,
                Pagination = new Pagination { PerPage = 20, Page = 1 },
            }
        );

        await Assert.That(ticketSearch).IsNotNull();
        await Assert.That(ticketSearch).IsNotEmpty();
        await Assert.That(ticketSearch).Contains(t => (t.Title ?? "").EndsWith(Id, StringComparison.OrdinalIgnoreCase));
    }
}
