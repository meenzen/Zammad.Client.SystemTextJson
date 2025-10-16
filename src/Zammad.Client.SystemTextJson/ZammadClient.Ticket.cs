using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public interface ITicketService
{
    Task<List<Ticket>> ListTicketsAsync();
    Task<List<Ticket>> ListTicketsAsync(int page, int count);
    Task<List<Ticket>> SearchTicketsAsync(string query, int limit);
    Task<List<Ticket>> SearchTicketsAsync(string query, int limit, string sortBy, string orderBy);
    Task<Ticket?> GetTicketAsync(TicketId id);
    Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article);
    Task<Ticket> UpdateTicketAsync(TicketId id, Ticket ticket);
    Task<bool> DeleteTicketAsync(TicketId id);
}

public sealed partial class ZammadClient : ITicketService
{
    public async Task<List<Ticket>> ListTicketsAsync() => await GetAsync<List<Ticket>>("/api/v1/tickets") ?? [];

    public async Task<List<Ticket>> ListTicketsAsync(int page, int count) =>
        await GetAsync<List<Ticket>>("/api/v1/tickets", $"page={page}&per_page={count}") ?? [];

    public async Task<List<Ticket>> SearchTicketsAsync(string query, int limit) =>
        await GetAsync<List<Ticket>>("/api/v1/tickets/search", $"query={query}&limit={limit}&expand=true") ?? [];

    public async Task<List<Ticket>> SearchTicketsAsync(string query, int limit, string sortBy, string orderBy) =>
        await GetAsync<List<Ticket>>(
            "/api/v1/tickets/search",
            $"query={query}&limit={limit}&expand=true&sort_by={sortBy}&order_by={orderBy}"
        ) ?? [];

    public async Task<Ticket?> GetTicketAsync(TicketId id) => await GetAsync<Ticket>($"/api/v1/tickets/{id}");

    public async Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article) =>
        await PostAsync<Ticket>("/api/v1/tickets", ticket.Combine(article))
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<Ticket> UpdateTicketAsync(TicketId id, Ticket ticket) =>
        await PutAsync<Ticket>($"/api/v1/tickets/{id}", ticket) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteTicketAsync(TicketId id) => await DeleteAsync<bool>($"/api/v1/tickets/{id}");
}
