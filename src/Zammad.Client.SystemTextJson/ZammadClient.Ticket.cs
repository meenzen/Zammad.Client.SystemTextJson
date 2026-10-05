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

    /// <summary>
    /// Changes only the title of a ticket.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="UpdateTicketAsync"/>, this skips the validation of required custom attributes, so it also
    /// works on tickets that were created before such an attribute existed. Triggers still run.
    /// </remarks>
    Task<Ticket> UpdateTicketTitleAsync(TicketId id, string title);

    /// <summary>
    /// Changes only the customer, and optionally the organization, of a ticket.
    /// </summary>
    /// <remarks>
    /// Skips the validation of required custom attributes like <see cref="UpdateTicketTitleAsync"/>. If the ticket's
    /// organization isn't one of the new customer's organizations, Zammad replaces it with the customer's primary
    /// organization.
    /// </remarks>
    Task<Ticket> UpdateTicketCustomerAsync(TicketId id, UserId customerId, OrganizationId? organizationId = null);

    Task DeleteTicketAsync(TicketId id);
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

    public async Task<Ticket> UpdateTicketTitleAsync(TicketId id, string title) =>
        await PutAsync<Ticket>($"{TicketsEndpoint}/{id}/update_title", new TicketTitleRequest { Title = title })
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<Ticket> UpdateTicketCustomerAsync(
        TicketId id,
        UserId customerId,
        OrganizationId? organizationId = null
    ) =>
        await PutAsync<Ticket>(
            $"{TicketsEndpoint}/{id}/update_customer",
            new TicketCustomerRequest { CustomerId = customerId, OrganizationId = organizationId }
        ) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteTicketAsync(TicketId id) => await DeleteAsync<bool>($"{TicketsEndpoint}/{id}");
}
