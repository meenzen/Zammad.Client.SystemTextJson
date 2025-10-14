using System.Threading.Tasks;
using Xunit;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[TestCaseOrderer(typeof(TestOrderer))]
public class TicketClientTest(ZammadStackFixture zammadStack)
{
    [Fact, Order(TestOrder.TicketCreate)]
    public async Task Ticket_Create_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = "Help me!",
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
                OwnerId = new UserId(1),
            },
            new TicketArticle
            {
                Subject = "Help me!!!",
                Body = "Nothing Work!",
                Type = "note",
            }
        );

        Assert.NotNull(ticket);
    }

    [Fact, Order(TestOrder.TicketSearch)]
    public async Task Ticket_Search_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var ticketSearch = await client.SearchTicketAsync("Zammad", 20);
        Assert.NotNull(ticketSearch);
    }
}
