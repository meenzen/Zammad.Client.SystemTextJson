using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

#nullable enable
public interface ITicketService
{
    Task<List<Ticket>> GetTicketListAsync();
    Task<List<Ticket>> GetTicketListAsync(int page, int count);
    Task<List<Ticket>> SearchTicketAsync(string query, int limit);
    Task<List<Ticket>> SearchTicketAsync(string query, int limit, string sortBy, string orderBy);
    Task<Ticket?> GetTicketAsync(int id);
    Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article);
    Task<Ticket> UpdateTicketAsync(int id, Ticket ticket);
    Task<bool> DeleteTicketAsync(int id);
}

public sealed partial class ZammadClient : ITicketService
{
    public async Task<List<Ticket>> GetTicketListAsync() => await GetAsync<List<Ticket>>("/api/v1/tickets") ?? [];

    public async Task<List<Ticket>> GetTicketListAsync(int page, int count) =>
        await GetAsync<List<Ticket>>("/api/v1/tickets", $"page={page}&per_page={count}") ?? [];

    public async Task<List<Ticket>> SearchTicketAsync(string query, int limit) =>
        await GetAsync<List<Ticket>>("/api/v1/tickets/search", $"query={query}&limit={limit}&expand=true") ?? [];

    public async Task<List<Ticket>> SearchTicketAsync(string query, int limit, string sortBy, string orderBy) =>
        await GetAsync<List<Ticket>>(
            "/api/v1/tickets/search",
            $"query={query}&limit={limit}&expand=true&sort_by={sortBy}&order_by={orderBy}"
        ) ?? [];

    public async Task<Ticket?> GetTicketAsync(int id) => await GetAsync<Ticket>($"/api/v1/tickets/{id}");

    public async Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article) =>
        await PostAsync<Ticket>("/api/v1/tickets", ticket.Combine(article))
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<Ticket> UpdateTicketAsync(int id, Ticket ticket) =>
        await PutAsync<Ticket>($"/api/v1/tickets/{id}", ticket) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteTicketAsync(int id) => await DeleteAsync<bool>($"/api/v1/tickets/{id}");
}
