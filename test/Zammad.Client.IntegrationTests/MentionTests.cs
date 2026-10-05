using System.Net;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class MentionTests(ZammadStackFixture zammadStack)
{
    private const string AgentLogin = "agent1@example.org";
    private static readonly string Id = TestSetup.RandomString();
    private static TicketId TicketId { get; set; } = TicketId.Empty;

    [Test]
    public async Task CreateTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = $"MentionTests {Id}",
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
            },
            new TicketArticle
            {
                Subject = "MentionTests",
                Body = "MentionTests",
                Type = "note",
            }
        );
        TicketId = ticket.Id;

        await Assert.That(await client.ListTicketMentionsAsync(TicketId)).IsEmpty();
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    public async Task CreateTicketMention()
    {
        var client = await zammadStack.GetClientAsync();
        var me = await client.GetUserMeAsync();

        await client.CreateTicketMentionAsync(TicketId);
        // Subscribing again does nothing
        await client.CreateTicketMentionAsync(TicketId);

        var mentions = await client.ListTicketMentionsAsync(TicketId);
        await Assert.That(mentions).HasSingleItem();
        var mention = mentions[0];
        await Assert.That(mention.UserId).IsEqualTo(me.Id);
        await Assert.That(mention.MentionableType).IsEqualTo("Ticket");
        await Assert.That(mention.MentionableId).IsEqualTo(TicketId.ToTargetObjectId());
        await Assert.That(mention.CreatedAt).IsNotNull();
    }

    [Test]
    [DependsOn(nameof(CreateTicketMention))]
    public async Task CreateTicketMention_OnBehalfOfOtherUser()
    {
        var client = await zammadStack.GetClientAsync();
        var agentClient = await zammadStack.GetClientOnBehalfOfAsync(AgentLogin);
        var agent = await agentClient.GetUserMeAsync();

        await agentClient.CreateTicketMentionAsync(TicketId);

        var mentions = await client.ListTicketMentionsAsync(TicketId);
        await Assert.That(mentions.Count).IsEqualTo(2);
        await Assert.That(mentions).Contains(m => m.UserId == agent.Id);
    }

    [Test]
    [DependsOn(nameof(CreateTicketMention_OnBehalfOfOtherUser))]
    public async Task DeleteMention_OfOtherUserIsForbidden()
    {
        var client = await zammadStack.GetClientAsync();
        var agentClient = await zammadStack.GetClientOnBehalfOfAsync(AgentLogin);
        var agent = await agentClient.GetUserMeAsync();
        var agentMention = (await client.ListTicketMentionsAsync(TicketId)).Single(m => m.UserId == agent.Id);

        var exception = await Assert.ThrowsAsync<ZammadException>(() => client.DeleteMentionAsync(agentMention.Id));
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.Forbidden);

        await agentClient.DeleteMentionAsync(agentMention.Id);

        await Assert.That(await client.ListTicketMentionsAsync(TicketId)).DoesNotContain(m => m.UserId == agent.Id);
    }

    [Test]
    [DependsOn(nameof(DeleteMention_OfOtherUserIsForbidden))]
    public async Task DeleteMention()
    {
        var client = await zammadStack.GetClientAsync();
        var mention = (await client.ListTicketMentionsAsync(TicketId)).Single();

        await client.DeleteMentionAsync(mention.Id);

        await Assert.That(await client.ListTicketMentionsAsync(TicketId)).IsEmpty();
    }

    [Test]
    public async Task ListTicketMentions_MissingTicketIsForbidden()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.ListTicketMentionsAsync(new TicketId(int.MaxValue))
        );
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    [DependsOn(nameof(DeleteMention))]
    public async Task DeleteTicket()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTicketAsync(TicketId);
    }
}
