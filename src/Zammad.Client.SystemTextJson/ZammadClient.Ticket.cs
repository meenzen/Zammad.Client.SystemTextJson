using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public interface ITicketService
{
    Task<List<Ticket>> ListTicketsAsync();

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<Ticket>> ListTicketsAsync(int page, int count);

    Task<List<Ticket>> ListTicketsAsync(Pagination? pagination);

    [Obsolete($"Use {nameof(SearchQuery)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<Ticket>> SearchTicketsAsync(string query, int limit);

    [Obsolete($"Use {nameof(SearchQuery)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<Ticket>> SearchTicketsAsync(string query, int limit, string sortBy, string orderBy);

    Task<List<Ticket>> SearchTicketsAsync(SearchQuery query);

    Task<Ticket?> GetTicketAsync(TicketId id);
    Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article);
    Task<Ticket> UpdateTicketAsync(TicketId id, Ticket ticket);
    Task<bool> DeleteTicketAsync(TicketId id);
}

public sealed partial class ZammadClient : ITicketService
{
    private const string TicketsEndpoint = "/api/v1/tickets";
    private const string TicketsSearchEndpoint = "/api/v1/tickets/search";

    public async Task<List<Ticket>> ListTicketsAsync() => await GetAsync<List<Ticket>>(TicketsEndpoint) ?? [];

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<Ticket>> ListTicketsAsync(int page, int count) =>
        await ListTicketsAsync(new Pagination { Page = page, PerPage = count });

    public async Task<List<Ticket>> ListTicketsAsync(Pagination? pagination)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Ticket>>(TicketsEndpoint, builder.ToString()) ?? [];
    }

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<Ticket>> SearchTicketsAsync(string query, int limit)
    {
        var builder = new QueryBuilder();
        builder.Add("query", query);
        builder.Add("limit", limit);
        builder.Add("expand", true);
        return await GetAsync<List<Ticket>>(TicketsSearchEndpoint, builder.ToString()) ?? [];
    }

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<Ticket>> SearchTicketsAsync(string query, int limit, string sortBy, string orderBy)
    {
        var builder = new QueryBuilder();
        builder.Add("query", query);
        builder.Add("limit", limit);
        builder.Add("expand", true);
        builder.Add("sort_by", sortBy);
        builder.Add("order_by", orderBy);
        return await GetAsync<List<Ticket>>(TicketsSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Ticket>> SearchTicketsAsync(SearchQuery query)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", true);
        return await GetAsync<List<Ticket>>(TicketsSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Ticket?> GetTicketAsync(TicketId id) => await GetAsync<Ticket>($"{TicketsEndpoint}/{id}");

    public async Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article) =>
        await PostAsync<Ticket>(TicketsEndpoint, ticket.Combine(article)) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Ticket> UpdateTicketAsync(TicketId id, Ticket ticket) =>
        await PutAsync<Ticket>($"{TicketsEndpoint}/{id}", ticket) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteTicketAsync(TicketId id) => await DeleteAsync<bool>($"{TicketsEndpoint}/{id}");
}
