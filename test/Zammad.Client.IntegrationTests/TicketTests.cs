using System;
using System.Threading.Tasks;
using Xunit;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[TestCaseOrderer(typeof(TestOrderer))]
public class TicketTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();

    [Fact, Order(TestOrder.TicketCreate)]
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

        Assert.NotNull(ticket);
    }

    [Fact, Order(TestOrder.TicketSearch)]
    public async Task Ticket_Search_Test()
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, TestContext.Current.CancellationToken);
        var ticketSearch = await client.SearchTicketsAsync(
            new SearchQuery
            {
                Query = Id,
                Pagination = new Pagination { PerPage = 20, Page = 1 },
            }
        );

        Assert.NotNull(ticketSearch);
        Assert.NotEmpty(ticketSearch);
        Assert.Contains(ticketSearch, t => (t.Title ?? "").EndsWith(Id, StringComparison.OrdinalIgnoreCase));
    }
}
