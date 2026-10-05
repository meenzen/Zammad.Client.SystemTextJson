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

    /// <summary>
    /// Updates an article. Unless Zammad runs in import mode, it only applies <see cref="TicketArticle.Internal"/> and
    /// the <c>highlight</c> key of <see cref="TicketArticle.Preferences"/>, and ignores all other fields.
    /// </summary>
    Task<TicketArticle> UpdateTicketArticleAsync(ArticleId id, TicketArticle article);

    /// <summary>
    /// Deletes an article. Zammad only lets agents delete internal notes that they created themselves, and only
    /// within the time frame set in <c>ui_ticket_zoom_article_delete_timeframe</c>.
    /// </summary>
    Task DeleteTicketArticleAsync(ArticleId id);

    Task<Stream?> GetTicketArticleAttachmentAsync(TicketId ticketId, ArticleId articleId, AttachmentId id);

    /// <summary>
    /// Returns the raw email (<c>message/rfc822</c>) of an article, or <c>null</c> if the article has no raw copy,
    /// e.g. because it's a note.
    /// </summary>
    Task<Stream?> GetTicketArticlePlainAsync(ArticleId id);
}

public sealed partial class ZammadClient : ITicketArticleService
{
    private const string TicketArticlesEndpoint = "/api/v1/ticket_articles";
    private const string TicketAttachmentEndpoint = "/api/v1/ticket_attachment";
    private const string TicketArticlePlainEndpoint = "/api/v1/ticket_article_plain";

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

    public async Task<TicketArticle> UpdateTicketArticleAsync(ArticleId id, TicketArticle article) =>
        await PutAsync<TicketArticle>($"{TicketArticlesEndpoint}/{id}", article)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteTicketArticleAsync(ArticleId id) =>
        await DeleteAsync<object>($"{TicketArticlesEndpoint}/{id}");

    public async Task<Stream?> GetTicketArticleAttachmentAsync(
        TicketId ticketId,
        ArticleId articleId,
        AttachmentId id
    ) => await GetAsync<Stream>($"{TicketAttachmentEndpoint}/{ticketId}/{articleId}/{id}");

    public async Task<Stream?> GetTicketArticlePlainAsync(ArticleId id) =>
        await GetAsync<Stream>($"{TicketArticlePlainEndpoint}/{id}");
}
