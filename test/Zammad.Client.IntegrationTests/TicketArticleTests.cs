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
    [Obsolete("Testing legacy pagination.")]
    public async Task ListTicketArticles_Pagination_Legacy()
    {
        var client = await zammadStack.GetClientAsync();

        var articles = await client.ListTicketArticlesAsync(1, 100);

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
    [DependsOn(nameof(ListTicketArticles_Pagination_Legacy))]
    [DependsOn(nameof(ListTicketArticlesByTicket))]
    public async Task GetTicketArticle()
    {
        var client = await zammadStack.GetClientAsync();

        var article = await client.GetTicketArticleAsync(TestArticleId);

        await Assert.That(article).IsNotNull();
        await Assert.That(article!.Id).IsEqualTo(TestArticleId);
        await Assert.That(article.TicketId).IsEqualTo(TestTicketId);
        await Assert.That(article.Subject).IsEqualTo("Test Article " + Id);
    }

    [Test]
    [DependsOn(nameof(GetTicketArticle))]
    public async Task GetTicketArticleAttachment()
    {
        var client = await zammadStack.GetClientAsync();

        var stream = await client.GetTicketArticleAttachmentAsync(TestTicketId, TestArticleId, TestAttachmentId);

        await Assert.That(stream).IsNotNull();
        await Assert.That(stream!.Length).IsGreaterThan(0);
    }
}
