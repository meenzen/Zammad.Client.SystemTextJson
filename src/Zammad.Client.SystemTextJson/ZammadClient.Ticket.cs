using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zammad.Client.Abstractions;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public sealed partial class ZammadClient
    : ITicketService,
        ITicketArticleService,
        ITicketPriorityService,
        ITicketStateService
{
    #region ITicketService

    public Task<List<Ticket>> GetTicketListAsync() => GetAsync<List<Ticket>>("/api/v1/tickets");

    public Task<List<Ticket>> GetTicketListAsync(int page, int count) =>
        GetAsync<List<Ticket>>("/api/v1/tickets", $"page={page}&per_page={count}");

    public Task<List<Ticket>> SearchTicketAsync(string query, int limit) =>
        GetAsync<List<Ticket>>("/api/v1/tickets/search", $"query={query}&limit={limit}&expand=true");

    public Task<List<Ticket>> SearchTicketAsync(string query, int limit, string sortBy, string orderBy) =>
        GetAsync<List<Ticket>>(
            "/api/v1/tickets/search",
            $"query={query}&limit={limit}&expand=true&sort_by={sortBy}&order_by={orderBy}"
        );

    public Task<Ticket> GetTicketAsync(int id) => GetAsync<Ticket>($"/api/v1/tickets/{id}");

    public Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article) =>
        PostAsync<Ticket>("/api/v1/tickets", ticket.Combine(article));

    public Task<Ticket> UpdateTicketAsync(int id, Ticket ticket) => PutAsync<Ticket>($"/api/v1/tickets/{id}", ticket);

    public Task<bool> DeleteTicketAsync(int id) => DeleteAsync<bool>($"/api/v1/tickets/{id}");

    #endregion

    #region ITicketArticleService

    public Task<List<TicketArticle>> GetTicketArticleListAsync() =>
        GetAsync<List<TicketArticle>>("/api/v1/ticket_articles");

    public Task<List<TicketArticle>> GetTicketArticleListAsync(int page, int count) =>
        GetAsync<List<TicketArticle>>("/api/v1/ticket_articles", $"page={page}&per_page={count}");

    public Task<List<TicketArticle>> GetTicketArticleListForTicketAsync(int ticketId) =>
        GetAsync<List<TicketArticle>>($"/api/v1/ticket_articles/by_ticket/{ticketId}");

    public Task<TicketArticle> GetTicketArticleAsync(int id) =>
        GetAsync<TicketArticle>($"/api/v1/ticket_articles/{id}");

    public Task<TicketArticle> CreateTicketArticleAsync(TicketArticle article) =>
        PostAsync<TicketArticle>("/api/v1/ticket_articles", article);

    public Task<Stream> GetTicketArticleAttachmentAsync(int ticketId, int articleId, int id) =>
        GetAsync<Stream>($"/api/v1/ticket_attachment/{ticketId}/{articleId}/{id}");

    #endregion

    #region ITicketPriorityService

    public Task<List<TicketPriority>> GetTicketPriorityListAsync() =>
        GetAsync<List<TicketPriority>>("/api/v1/ticket_priorities");

    public Task<List<TicketPriority>> GetTicketPriorityListAsync(int page, int count) =>
        GetAsync<List<TicketPriority>>("/api/v1/ticket_priorities", $"page={page}&per_page={count}");

    public Task<TicketPriority> GetTicketPriorityAsync(int id) =>
        GetAsync<TicketPriority>($"/api/v1/ticket_priorities/{id}");

    public Task<TicketPriority> CreateTicketPriorityAsync(TicketPriority priority) =>
        PostAsync<TicketPriority>("/api/v1/ticket_priorities", priority);

    public Task<TicketPriority> UpdateTicketPriorityAsync(int id, TicketPriority priority) =>
        PutAsync<TicketPriority>($"/api/v1/ticket_priorities/{id}", priority);

    public Task<bool> DeleteTicketPriorityAsync(int id) => DeleteAsync<bool>($"/api/v1/ticket_priorities/{id}");

    #endregion

    #region ITicketStateService

    public Task<List<TicketState>> GetTicketStateListAsync() => GetAsync<List<TicketState>>("/api/v1/ticket_states");

    public Task<List<TicketState>> GetTicketStateListAsync(int page, int count) =>
        GetAsync<List<TicketState>>("/api/v1/ticket_states", $"page={page}&per_page={count}");

    public Task<TicketState> GetTicketStateAsync(int id) => GetAsync<TicketState>($"/api/v1/ticket_states/{id}");

    public Task<TicketState> CreateTicketStateAsync(TicketState state) =>
        PostAsync<TicketState>("/api/v1/ticket_states", state);

    public Task<TicketState> UpdateTicketStateAsync(int id, TicketState state) =>
        PutAsync<TicketState>($"/api/v1/ticket_states/{id}", state);

    public Task<bool> DeleteTicketStateAsync(int id) => DeleteAsync<bool>($"/api/v1/ticket_states/{id}");

    #endregion
}
