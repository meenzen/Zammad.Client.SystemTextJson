using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public interface ITicketService
{
    Task<List<Ticket>> ListTicketsAsync(Pagination? pagination = null);
    Task<List<Ticket>> SearchTicketsAsync(SearchQuery query, bool expand = true);
    Task<Ticket?> GetTicketAsync(TicketId id);
    Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article);
    Task<Ticket> UpdateTicketAsync(TicketId id, Ticket ticket);
    Task<bool> DeleteTicketAsync(TicketId id);
}

public sealed partial class ZammadClient : ITicketService
{
    private const string TicketsEndpoint = "/api/v1/tickets";
    private const string TicketsSearchEndpoint = "/api/v1/tickets/search";

    public async Task<List<Ticket>> ListTicketsAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Ticket>>(TicketsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Ticket>> SearchTicketsAsync(SearchQuery query, bool expand = true)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", expand);
        return await GetAsync<List<Ticket>>(TicketsSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Ticket?> GetTicketAsync(TicketId id) => await GetAsync<Ticket>($"{TicketsEndpoint}/{id}");

    public async Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article) =>
        await PostAsync<Ticket>(TicketsEndpoint, ticket.Combine(article)) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Ticket> UpdateTicketAsync(TicketId id, Ticket ticket) =>
        await PutAsync<Ticket>($"{TicketsEndpoint}/{id}", ticket) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteTicketAsync(TicketId id) => await DeleteAsync<bool>($"{TicketsEndpoint}/{id}");
}
