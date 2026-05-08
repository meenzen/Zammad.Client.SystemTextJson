using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TicketAccountingTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();
    private static TicketId TestTicketId { get; set; } = TicketId.Empty;
    private static TimeAccountingId TestAccountingId { get; set; } = TimeAccountingId.Empty;

    [Test]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task CreateTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = "Accounting Test Ticket " + Id,
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
                OwnerId = new UserId(1),
            },
            new TicketArticle
            {
                Subject = "Initial Article " + Id,
                Body = "Initial article body " + Id,
                Type = "note",
            }
        );

        await Assert.That(ticket).IsNotNull();
        await Assert.That(ticket.Id).IsNotEqualTo(TicketId.Empty);

        TestTicketId = ticket.Id;
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    public async Task CreateTicketAccounting()
    {
        var client = await zammadStack.GetClientAsync();

        var accounting = await client.CreateTicketAccountingAsync(
            TestTicketId,
            new TicketAccounting { TimeUnit = "60.0" }
        );

        await Assert.That(accounting).IsNotNull();
        await Assert.That(accounting.Id).IsNotEqualTo(TimeAccountingId.Empty);
        await Assert.That(accounting.TicketId).IsEqualTo(TestTicketId);
        await Assert.That(accounting.TimeUnit).IsEqualTo("60.0");

        TestAccountingId = accounting.Id;
    }

    [Test]
    [DependsOn(nameof(CreateTicketAccounting))]
    public async Task ListTicketAccountings()
    {
        var client = await zammadStack.GetClientAsync();

        var accountings = await client.ListTicketAccountingsAsync(TestTicketId);

        await Assert.That(accountings).IsNotEmpty();
        await Assert.That(accountings).Contains(a => a.Id == TestAccountingId);
    }

    [Test]
    [DependsOn(nameof(CreateTicketAccounting))]
    public async Task GetTicketAccounting()
    {
        var client = await zammadStack.GetClientAsync();

        var accounting = await client.GetTicketAccountingAsync(TestTicketId, TestAccountingId);

        await Assert.That(accounting).IsNotNull();
        await Assert.That(accounting!.Id).IsEqualTo(TestAccountingId);
        await Assert.That(accounting.TicketId).IsEqualTo(TestTicketId);
        await Assert.That(accounting.TimeUnit).IsEqualTo("60.0");
    }

    [Test]
    [DependsOn(nameof(GetTicketAccounting))]
    [DependsOn(nameof(ListTicketAccountings))]
    public async Task UpdateTicketAccounting()
    {
        var client = await zammadStack.GetClientAsync();

        var updated = await client.UpdateTicketAccountingAsync(
            TestTicketId,
            TestAccountingId,
            new TicketAccounting { TimeUnit = "30.0" }
        );

        await Assert.That(updated).IsNotNull();
        await Assert.That(updated.Id).IsEqualTo(TestAccountingId);
        await Assert.That(updated.TimeUnit).IsEqualTo("30.0");
    }

    [Test]
    [DependsOn(nameof(UpdateTicketAccounting))]
    public async Task DeleteTicketAccounting()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.DeleteTicketAccountingAsync(TestTicketId, TestAccountingId);
        await Assert.That(result).IsTrue();

        var deleted = await client.GetTicketAccountingAsync(TestTicketId, TestAccountingId);
        await Assert.That(deleted).IsNull();
    }
}
