using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TicketArticleTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();
    private static TicketId TestTicketId { get; set; } = TicketId.Empty;
    private static ArticleId TestArticleId { get; set; } = ArticleId.Empty;
    private static AttachmentId TestAttachmentId { get; set; } = AttachmentId.Empty;

    [Test]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task CreateTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = "Article Test Ticket " + Id,
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
    public async Task CreateTicketArticle()
    {
        var client = await zammadStack.GetClientAsync();

        var attachmentData = "Hello, attachment!"u8.ToArray();
        var article = await client.CreateTicketArticleAsync(
            new TicketArticle
            {
                TicketId = TestTicketId,
                Subject = "Test Article " + Id,
                Body = "Test article body " + Id,
                Type = "note",
                Attachments = [TicketAttachment.CreateFromBytes(attachmentData, "test.txt", "text/plain")],
            }
        );

        await Assert.That(article).IsNotNull();
        await Assert.That(article.Id).IsNotEqualTo(ArticleId.Empty);
        await Assert.That(article.TicketId).IsEqualTo(TestTicketId);
        await Assert.That(article.Subject).IsEqualTo("Test Article " + Id);
        await Assert.That(article.Attachments).IsNotNull();
        await Assert.That(article.Attachments).IsNotEmpty();

        TestArticleId = article.Id;
        TestAttachmentId = article.Attachments![0].Id;
    }

    [Test]
    [DependsOn(nameof(CreateTicketArticle))]
    public async Task ListTicketArticles()
    {
        var client = await zammadStack.GetClientAsync();

        var articles = await client.ListTicketArticlesAsync();

        await Assert.That(articles).IsNotEmpty();
        await Assert.That(articles).Contains(a => a.Id == TestArticleId);
    }

    [Test]
    [DependsOn(nameof(CreateTicketArticle))]
    public async Task ListTicketArticles_Pagination()
    {
        var client = await zammadStack.GetClientAsync();

        var articles = await client.ListTicketArticlesAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(articles).IsNotEmpty();
        await Assert.That(articles).Contains(a => a.Id == TestArticleId);
    }

    [Test]
    [DependsOn(nameof(CreateTicketArticle))]
    public async Task ListTicketArticlesByTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var articles = await client.ListTicketArticlesAsync(TestTicketId);

        await Assert.That(articles).IsNotEmpty();
        await Assert.That(articles).Contains(a => a.Id == TestArticleId);
    }

    [Test]
    [DependsOn(nameof(ListTicketArticles))]
    [DependsOn(nameof(ListTicketArticles_Pagination))]
    [DependsOn(nameof(ListTicketArticlesByTicket))]
    public async Task GetTicketArticle()
    {
        var client = await zammadStack.GetClientAsync();

        var article = await client.GetTicketArticleAsync(TestArticleId);

        await Assert.That(article).IsNotNull();
        await Assert.That(article!.Id).IsEqualTo(TestArticleId);
        await Assert.That(article.TicketId).IsEqualTo(TestTicketId);
        await Assert.That(article.Subject).IsEqualTo("Test Article " + Id);
        await Assert.That(article.Type).IsEqualTo("note");
        await Assert.That(article.TypeId).IsNotNull();
        await Assert.That(article.Sender).IsEqualTo("Agent");
        await Assert.That(article.SenderId).IsEqualTo(new ArticleSenderId(1));
    }

    [Test]
    [DependsOn(nameof(GetTicketArticle))]
    public async Task GetTicketArticleAttachment()
    {
        var client = await zammadStack.GetClientAsync();

        var stream = await client.GetTicketArticleAttachmentAsync(TestTicketId, TestArticleId, TestAttachmentId);

        await Assert.That(stream).IsNotNull();
        using var reader = new StreamReader(stream!);
        await Assert.That(await reader.ReadToEndAsync()).IsEqualTo("Hello, attachment!");
    }

    [Test]
    [DependsOn(nameof(GetTicketArticle))]
    public async Task GetTicketArticlePlain_NoRawCopy()
    {
        var client = await zammadStack.GetClientAsync();

        // Only emails have a raw copy. The stack can't send or receive any, so only the note case is tested.
        var stream = await client.GetTicketArticlePlainAsync(TestArticleId);

        await Assert.That(stream).IsNull();
    }

    [Test]
    [DependsOn(nameof(GetTicketArticle))]
    public async Task UpdateTicketArticle()
    {
        var client = await zammadStack.GetClientAsync();

        var updated = await client.UpdateTicketArticleAsync(TestArticleId, new TicketArticle { Internal = true });

        await Assert.That(updated.Id).IsEqualTo(TestArticleId);
        await Assert.That(updated.Internal).IsTrue();

        var article = await client.GetTicketArticleAsync(TestArticleId);
        await Assert.That(article).IsNotNull();
        await Assert.That(article!.Internal).IsTrue();
        await Assert.That(article.Subject).IsEqualTo("Test Article " + Id);
    }

    [Test]
    [DependsOn(nameof(GetTicketArticleAttachment))]
    [DependsOn(nameof(GetTicketArticlePlain_NoRawCopy))]
    [DependsOn(nameof(UpdateTicketArticle))]
    public async Task DeleteTicketArticle()
    {
        var client = await zammadStack.GetClientAsync();

        // Agents can delete their own notes
        await client.DeleteTicketArticleAsync(TestArticleId);

        await Assert.That(await client.GetTicketArticleAsync(TestArticleId)).IsNull();
        var articles = await client.ListTicketArticlesAsync(TestTicketId);
        await Assert.That(articles).DoesNotContain(a => a.Id == TestArticleId);
    }
}
