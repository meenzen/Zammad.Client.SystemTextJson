using System.Net;
using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TicketMassTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();
    private static List<Ticket> Tickets { get; } = [];
    private static User? Customer { get; set; }
    private static Macro? Macro { get; set; }
    private static Macro? RestrictedMacro { get; set; }

    private static List<TicketId> TicketIds => Tickets.ConvertAll(t => t.Id);

    [Test]
    public async Task CreateRecords()
    {
        var client = await zammadStack.GetClientAsync();

        Customer = await client.CreateUserAsync(
            new User
            {
                FirstName = "Mass",
                LastName = $"Customer {Id}",
                Email = $"mass.{Id}@example.org",
            }
        );

        for (var i = 0; i < 2; i++)
        {
            Tickets.Add(
                await client.CreateTicketAsync(
                    new Ticket
                    {
                        Title = $"TicketMassTests {i} {Id}",
                        GroupId = new GroupId(1),
                        CustomerId = new UserId(1),
                        PriorityId = new PriorityId(2),
                    },
                    new TicketArticle
                    {
                        Subject = "TicketMassTests",
                        Body = "TicketMassTests",
                        Type = "note",
                    }
                )
            );
        }

        Macro = await client.CreateMacroAsync(
            new Macro
            {
                Name = $"TicketMassTests {Id}",
                Perform = JsonSerializer.SerializeToElement(
                    new Dictionary<string, object>
                    {
                        ["ticket.priority_id"] = new { value = "1" },
                        ["ticket.tags"] = new { @operator = "add", value = $"mass{Id}" },
                    }
                ),
            }
        );

        var otherGroup = (await client.ListGroupsAsync()).Find(g => g.Id != new GroupId(1));
        await Assert.That(otherGroup).IsNotNull();
        RestrictedMacro = await client.CreateMacroAsync(
            new Macro
            {
                Name = $"TicketMassTests restricted {Id}",
                Perform = JsonSerializer.SerializeToElement(
                    new Dictionary<string, object> { ["ticket.priority_id"] = new { value = "3" } }
                ),
                GroupIds = [otherGroup!.Id],
            }
        );
    }

    [Test]
    [DependsOn(nameof(CreateRecords))]
    public async Task MassUpdateTickets()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.MassUpdateTicketsAsync(
            TicketIds,
            new Ticket { PriorityId = new PriorityId(3) },
            new TicketArticle
            {
                Subject = "Mass update",
                Body = $"Mass update {Id}",
                Type = "note",
                Internal = true,
            }
        );

        await Assert.That(result.TicketIds).IsEquivalentTo(TicketIds);
        var tickets = result.GetTickets();
        await Assert.That(tickets.Count).IsEqualTo(2);
        await Assert.That(tickets).All(t => t.PriorityId == new PriorityId(3));

        foreach (var ticket in Tickets)
        {
            var articles = await client.ListTicketArticlesAsync(ticket.Id);
            await Assert.That(articles).Contains(a => a.Body == $"Mass update {Id}" && a.Internal == true);
        }
    }

    [Test]
    [DependsOn(nameof(MassUpdateTickets))]
    public async Task MassUpdateTickets_NotAuthorized()
    {
        // A customer has no change access to the tickets
        var client = await zammadStack.GetClientOnBehalfOfAsync(Customer!.Email!);

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.MassUpdateTicketsAsync(TicketIds, new Ticket { PriorityId = new PriorityId(1) })
        );
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.GetFailedTicketId()).IsEqualTo(Tickets[0].Id);
        await Assert.That(exception.Error).IsNull();

        var admin = await zammadStack.GetClientAsync();
        await Assert.That((await admin.GetTicketAsync(Tickets[0].Id))!.PriorityId).IsEqualTo(new PriorityId(3));
    }

    [Test]
    [DependsOn(nameof(CreateRecords))]
    public async Task MassUpdateTickets_MissingTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.MassUpdateTicketsAsync([Tickets[0].Id, new TicketId(int.MaxValue)], new Ticket { Note = "x" })
        );
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(exception.GetFailedTicketId()).IsNull();
    }

    [Test]
    [DependsOn(nameof(MassUpdateTickets_NotAuthorized))]
    [DependsOn(nameof(MassUpdateTickets_MissingTicket))]
    public async Task ApplyMacroToTickets()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.ApplyMacroToTicketsAsync(Macro!.Id, TicketIds);

        await Assert.That(result.TicketIds).IsEquivalentTo(TicketIds);
        await Assert.That(result.GetTickets()).All(t => t.PriorityId == new PriorityId(1));
        foreach (var ticket in Tickets)
        {
            var tags = await client.ListTagsAsync(ObjectType.Ticket, ticket.Id.ToTargetObjectId());
            await Assert.That(tags).Contains($"mass{Id}");
        }
    }

    [Test]
    [DependsOn(nameof(ApplyMacroToTickets))]
    public async Task ApplyMacroToTickets_GroupRestriction()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.ApplyMacroToTicketsAsync(RestrictedMacro!.Id, TicketIds)
        );
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("Macro group restrictions do not cover all tickets");
        await Assert.That(exception.GetBlockingTicketIds()).IsEquivalentTo(TicketIds);

        var missing = await Assert.ThrowsAsync<ZammadException>(() =>
            client.ApplyMacroToTicketsAsync(new MacroId(int.MaxValue), TicketIds)
        );
        await Assert.That(missing!.Code).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [DependsOn(nameof(ApplyMacroToTickets_GroupRestriction))]
    public async Task DeleteRecords()
    {
        var client = await zammadStack.GetClientAsync();

        foreach (var ticket in Tickets)
        {
            await client.DeleteTicketAsync(ticket.Id);
        }

        await client.DeleteMacroAsync(Macro!.Id);
        await client.DeleteMacroAsync(RestrictedMacro!.Id);
        await client.DeleteUserAsync(Customer!.Id);
    }
}
