using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TagTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();
    private static readonly string TagName = "IntegrationTestTag" + Id;
    private static TicketId? CreatedTicketId { get; set; }

    [Test]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task CreateTicketForTags()
    {
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = "Ticket with tags!" + Id,
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
        CreatedTicketId = ticket.Id;
    }

    [Test]
    [DependsOn(nameof(CreateTicketForTags))]
    public async Task AddTag()
    {
        await Assert.That(CreatedTicketId).IsNotNull();
        var client = await zammadStack.GetClientAsync();
        var result = await client.AddTagAsync(ObjectType.Ticket, new ObjectId(CreatedTicketId.Value.Value), TagName);
        await Assert.That(result).IsTrue();
    }

    [Test]
    [DependsOn(nameof(AddTag))]
    public async Task ListTags()
    {
        await Assert.That(CreatedTicketId).IsNotNull();
        var client = await zammadStack.GetClientAsync();
        var tagList = await client.ListTagsAsync(ObjectType.Ticket, new ObjectId(CreatedTicketId.Value.Value));
        await Assert.That(tagList).IsNotEmpty();
        await Assert.That(tagList).Contains(TagName);
    }

    [Test]
    [DependsOn(nameof(ListTags))]
    public async Task RemoveTag()
    {
        await Assert.That(CreatedTicketId).IsNotNull();
        var objectId = new ObjectId(CreatedTicketId.Value.Value);

        var client = await zammadStack.GetClientAsync();
        var result = await client.RemoveTagAsync(ObjectType.Ticket, objectId, TagName);
        await Assert.That(result).IsTrue();

        await Assert.That(CreatedTicketId).IsNotNull();
        var tagList = await client.ListTagsAsync(ObjectType.Ticket, objectId);
        await Assert.That(tagList).DoesNotContain(TagName);
    }
}
