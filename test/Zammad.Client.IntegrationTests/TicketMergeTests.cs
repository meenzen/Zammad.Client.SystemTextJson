using System.Net;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TicketMergeTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();
    private static Ticket? Source { get; set; }
    private static Ticket? Target { get; set; }

    private static async Task<Ticket> CreateTicketAsync(IZammadClient client, string name) =>
        await client.CreateTicketAsync(
            new Ticket
            {
                Title = $"TicketMergeTests {name} {Id}",
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
            },
            new TicketArticle
            {
                Subject = "TicketMergeTests",
                Body = $"TicketMergeTests {name}",
                Type = "note",
            }
        );

    [Test]
    public async Task CreateTickets()
    {
        var client = await zammadStack.GetClientAsync();

        Source = await CreateTicketAsync(client, "Source");
        Target = await CreateTicketAsync(client, "Target");
    }

    [Test]
    [DependsOn(nameof(CreateTickets))]
    public async Task MergeTicket_Invalid()
    {
        var client = await zammadStack.GetClientAsync();

        // Zammad reports missing tickets with 200 OK and {"result": "failed"}
        var missingTarget = await Assert.ThrowsAsync<ZammadException>(() =>
            client.MergeTicketAsync(Source!.Id, "does-not-exist")
        );
        await Assert.That(missingTarget!.Code).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(missingTarget.Error).IsEqualTo("The target ticket number could not be found.");
        await Assert.That(missingTarget.Message).EndsWith(": The target ticket number could not be found.");

        var missingSource = await Assert.ThrowsAsync<ZammadException>(() =>
            client.MergeTicketAsync(new TicketId(int.MaxValue), Target!.Number!)
        );
        await Assert.That(missingSource!.Code).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(missingSource.Error).IsEqualTo("The source ticket could not be found.");

        var self = await Assert.ThrowsAsync<ZammadException>(() => client.MergeTicketAsync(Source!.Id, Source.Number!));
        await Assert.That(self!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(self.Error).IsEqualTo("A ticket cannot be merged into itself.");
    }

    [Test]
    [DependsOn(nameof(MergeTicket_Invalid))]
    public async Task MergeTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.MergeTicketAsync(Source!.Id, Target!.Number!);

        await Assert.That(result.SourceTicket?.Id).IsEqualTo(Source.Id);
        await Assert.That(result.TargetTicket?.Id).IsEqualTo(Target.Id);

        var states = await client.ListTicketStatesAsync();
        var merged = states.Find(s => s.Name == "merged");
        await Assert.That(merged).IsNotNull();
        await Assert.That(result.SourceTicket!.StateId).IsEqualTo(merged!.Id);
        await Assert.That(result.SourceTicket.OwnerId).IsEqualTo(new UserId(1));

        // The articles were moved, and the source ticket got a "merged" note
        var targetArticles = await client.ListTicketArticlesAsync(Target.Id);
        await Assert.That(targetArticles).Contains(a => a.Body == "TicketMergeTests Source");
        var sourceArticles = await client.ListTicketArticlesAsync(Source.Id);
        await Assert.That(sourceArticles).HasSingleItem();
        await Assert.That(sourceArticles[0].Body).IsEqualTo("merged");

        // The target ticket is now the parent of the source ticket
        var links = await client.ListTicketLinksAsync(Source.Id);
        await Assert.That(links.Links).Contains(l => l.LinkType == LinkType.Parent && l.TicketId == Target.Id);

        // Both tickets log the merge in their history
        var targetHistory = await client.GetTicketHistoryAsync(Target.Id);
        var received = targetHistory!.Entries.Find(e => e.Type == "received_merge");
        await Assert.That(received).IsNotNull();
        await Assert.That(received!.IdFrom?.Value).IsEqualTo(Source.Id.Value);
        await Assert.That(received.IdTo?.Value).IsEqualTo(Target.Id.Value);
        var sourceHistory = await client.GetTicketHistoryAsync(Source.Id);
        await Assert.That(sourceHistory!.Entries).Contains(e => e.Type == "merged_into");
    }

    [Test]
    [DependsOn(nameof(MergeTicket))]
    public async Task MergeTicket_IntoMergedTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.MergeTicketAsync(Target!.Id, Source!.Number!)
        );
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("It is not possible to merge into an already merged ticket.");
    }

    [Test]
    [DependsOn(nameof(MergeTicket_IntoMergedTicket))]
    public async Task DeleteTickets()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTicketAsync(Source!.Id);
        await client.DeleteTicketAsync(Target!.Id);
    }
}
