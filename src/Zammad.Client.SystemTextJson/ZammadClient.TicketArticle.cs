using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface ITicketArticleService
{
    Task<List<TicketArticle>> ListTicketArticlesAsync();

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<TicketArticle>> ListTicketArticlesAsync(int page, int count);

    Task<List<TicketArticle>> ListTicketArticlesAsync(Pagination? pagination);
    Task<List<TicketArticle>> ListTicketArticlesAsync(TicketId id);
    Task<TicketArticle?> GetTicketArticleAsync(ArticleId id);
    Task<TicketArticle> CreateTicketArticleAsync(TicketArticle article);
    Task<Stream?> GetTicketArticleAttachmentAsync(TicketId ticketId, ArticleId articleId, AttachmentId id);
}

public sealed partial class ZammadClient : ITicketArticleService
{
    private const string TicketArticlesEndpoint = "/api/v1/ticket_articles";
    private const string TicketAttachmentEndpoint = "/api/v1/ticket_attachment";

    public async Task<List<TicketArticle>> ListTicketArticlesAsync() =>
        await GetAsync<List<TicketArticle>>(TicketArticlesEndpoint) ?? [];

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<TicketArticle>> ListTicketArticlesAsync(int page, int count) =>
        await ListTicketArticlesAsync(new Pagination { Page = page, PerPage = count });

    public async Task<List<TicketArticle>> ListTicketArticlesAsync(Pagination? pagination)
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
