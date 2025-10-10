using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface ITicketArticleService
{
    Task<List<TicketArticle>> GetTicketArticleListAsync();
    Task<List<TicketArticle>> GetTicketArticleListAsync(int page, int count);
    Task<List<TicketArticle>> GetTicketArticleListForTicketAsync(int ticketId);
    Task<TicketArticle?> GetTicketArticleAsync(int id);
    Task<TicketArticle> CreateTicketArticleAsync(TicketArticle article);
    Task<Stream?> GetTicketArticleAttachmentAsync(int ticketId, int articleId, int id);
}

public sealed partial class ZammadClient : ITicketArticleService
{
    public async Task<List<TicketArticle>> GetTicketArticleListAsync() =>
        await GetAsync<List<TicketArticle>>("/api/v1/ticket_articles") ?? [];

    public async Task<List<TicketArticle>> GetTicketArticleListAsync(int page, int count) =>
        await GetAsync<List<TicketArticle>>("/api/v1/ticket_articles", $"page={page}&per_page={count}") ?? [];

    public async Task<List<TicketArticle>> GetTicketArticleListForTicketAsync(int ticketId) =>
        await GetAsync<List<TicketArticle>>($"/api/v1/ticket_articles/by_ticket/{ticketId}") ?? [];

    public async Task<TicketArticle?> GetTicketArticleAsync(int id) =>
        await GetAsync<TicketArticle>($"/api/v1/ticket_articles/{id}");

    public async Task<TicketArticle> CreateTicketArticleAsync(TicketArticle article) =>
        await PostAsync<TicketArticle>("/api/v1/ticket_articles", article) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Stream?> GetTicketArticleAttachmentAsync(int ticketId, int articleId, int id) =>
        await GetAsync<Stream>($"/api/v1/ticket_attachment/{ticketId}/{articleId}/{id}");
}
