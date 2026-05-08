using System.Diagnostics.CodeAnalysis;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface ITicketArticleService
{
    Task<List<TicketArticle>> ListTicketArticlesAsync(Pagination? pagination = null);
    Task<List<TicketArticle>> ListTicketArticlesAsync(TicketId id);
    Task<TicketArticle?> GetTicketArticleAsync(ArticleId id);
    Task<TicketArticle> CreateTicketArticleAsync(TicketArticle article);
    Task<Stream?> GetTicketArticleAttachmentAsync(TicketId ticketId, ArticleId articleId, AttachmentId id);
}

public sealed partial class ZammadClient : ITicketArticleService
{
    private const string TicketArticlesEndpoint = "/api/v1/ticket_articles";
    private const string TicketAttachmentEndpoint = "/api/v1/ticket_attachment";

    public async Task<List<TicketArticle>> ListTicketArticlesAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<TicketArticle>>(TicketArticlesEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<TicketArticle>> ListTicketArticlesAsync(TicketId id) =>
        await GetAsync<List<TicketArticle>>($"{TicketArticlesEndpoint}/by_ticket/{id}") ?? [];

    public async Task<TicketArticle?> GetTicketArticleAsync(ArticleId id) =>
        await GetAsync<TicketArticle>($"{TicketArticlesEndpoint}/{id}");

    public async Task<TicketArticle> CreateTicketArticleAsync(TicketArticle article) =>
        await PostAsync<TicketArticle>(TicketArticlesEndpoint, article) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Stream?> GetTicketArticleAttachmentAsync(
        TicketId ticketId,
        ArticleId articleId,
        AttachmentId id
    ) => await GetAsync<Stream>($"{TicketAttachmentEndpoint}/{ticketId}/{articleId}/{id}");
}
