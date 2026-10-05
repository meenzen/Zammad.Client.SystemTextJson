using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Tests.Deserialization;

namespace Zammad.Client.Tests.Resources;

/// <summary>
/// The helpers that resolve the IDs of search, overview and mass results through <see cref="Assets"/>.
/// </summary>
public class ResultResolutionTests
{
    private static async Task<T> DeserializeAsync<T>(string fileName)
    {
        var json = await TestFile.ReadStringAsync("../Deserialization/Responses", fileName);
        return JsonSerializer.Deserialize<T>(json, Serialization.GetOptions()) ?? throw new InvalidOperationException();
    }

    [Test]
    public async Task SearchResult_ResolvesHits()
    {
        var result = await DeserializeAsync<SearchResult>("search.json");

        await Assert.That(result.Hits.Count).IsEqualTo(4);
        await Assert.That(result.Hits[0].TicketId).IsEqualTo(new TicketId(11));
        await Assert.That(result.Hits[0].UserId).IsNull();
        await Assert
            .That(result.GetTickets().ConvertAll(t => t.Id))
            .IsEquivalentTo([new TicketId(11), new TicketId(10)]);
        await Assert.That(result.GetUsers().ConvertAll(u => u.Id)).IsEquivalentTo([new UserId(7)]);
        await Assert.That(result.GetOrganizations().ConvertAll(o => o.Id)).IsEquivalentTo([new OrganizationId(5)]);
    }

    [Test]
    public async Task SearchResultByObject_ResolvesHits()
    {
        var result = await DeserializeAsync<SearchResultByObject>("searchByObject.json");

        await Assert.That(result.Get(SearchObjectType.Ticket)!.TotalCount).IsEqualTo(2);
        await Assert.That(result.Get(SearchObjectType.ChatSession)).IsNull();
        await Assert.That(result.GetTickets().ConvertAll(t => t.Id)).IsEquivalentTo([new TicketId(10)]);
        await Assert.That(result.GetUsers().ConvertAll(u => u.Id)).IsEquivalentTo([new UserId(7)]);
        await Assert.That(result.GetOrganizations().ConvertAll(o => o.Id)).IsEquivalentTo([new OrganizationId(5)]);
    }

    [Test]
    public async Task TicketOverviewResult_ResolvesTicketsInOrder()
    {
        var result = await DeserializeAsync<TicketOverviewResult>("ticketOverview.json");

        await Assert.That(result.Index.Overview!.View).IsEqualTo("all_unassigned");
        await Assert.That(result.Index.Count).IsEqualTo(12);
        var tickets = result.GetTickets();
        await Assert.That(tickets.Count).IsEqualTo(12);
        await Assert.That(tickets.ConvertAll(t => t.Id)).IsEquivalentTo(result.Index.Tickets.ConvertAll(t => t.Id));
        await Assert.That(tickets[0].Id).IsEqualTo(new TicketId(1));
        await Assert.That(result.Assets.GetOverview(new OverviewId(2))!.Link).IsEqualTo("all_unassigned");
    }

    [Test]
    public async Task TicketOverviewResult_Unknown()
    {
        var result = await DeserializeAsync<TicketOverviewResult>("ticketOverviewUnknown.json");

        await Assert.That(result.Index.Overview).IsNull();
        await Assert.That(result.GetTickets()).IsEmpty();
    }

    [Test]
    public async Task TicketMassResult_ResolvesTickets()
    {
        var result = await DeserializeAsync<TicketMassResult>("ticketMassMacro.json");

        await Assert
            .That(result.GetTickets().ConvertAll(t => t.Id))
            .IsEquivalentTo([new TicketId(16), new TicketId(17)]);
        await Assert.That(result.GetTickets()).All(t => t.PriorityId == new PriorityId(1));
    }

    [Test]
    public async Task HistoryList_MergeEntry()
    {
        var history = await DeserializeAsync<HistoryList>("ticketHistoryMerged.json");

        var merge = history.Entries.Find(e => e.Type == "merged_into");
        await Assert.That(merge).IsNotNull();
        await Assert.That(merge!.Id).IsEqualTo(new HistoryId(156));
        await Assert.That(merge.ObjectId).IsEqualTo(new TargetObjectId(18));
        await Assert.That(merge.IdFrom).IsEqualTo(new TargetObjectId(18));
        await Assert.That(merge.IdTo).IsEqualTo(new TargetObjectId(20));
        await Assert.That(merge.CreatedById).IsEqualTo(new UserId(3));
        await Assert.That(history.Assets.GetUser(new UserId(3))).IsNotNull();
    }
}
