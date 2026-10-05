using System.Text.Json;
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

    /// <summary>
    /// Gets the history of a ticket, including the changes of its articles, mentions, shared drafts and checklists.
    /// </summary>
    /// <remarks>
    /// Needs the <c>ticket.agent</c> permission and read access to the ticket. Returns <c>null</c> if the ticket
    /// doesn't exist.
    /// </remarks>
    Task<HistoryList?> GetTicketHistoryAsync(TicketId id);

    /// <summary>
    /// Merges a ticket into another ticket.
    /// </summary>
    /// <param name="sourceTicketId">The ticket to merge. It ends up in the <c>merged</c> state.</param>
    /// <param name="targetTicketNumber">
    /// The <see cref="Ticket.Number"/> (not the ID) of the ticket to merge into. It receives all articles, mentions
    /// and links of the source ticket and gets a parent link to it.
    /// </param>
    /// <remarks>
    /// <para>
    /// Needs the <c>ticket.agent</c> permission and change access to both tickets (403 otherwise).
    /// </para>
    /// <para>
    /// Zammad answers 200 OK with <c>{"result": "failed"}</c> if one of the tickets doesn't exist. This method throws a
    /// <see cref="ZammadException"/> for that, with Zammad's message ("The target ticket number could not be found."
    /// or "The source ticket could not be found.") in <see cref="ZammadException.Error"/>. Merging a ticket into
    /// itself ("A ticket cannot be merged into itself.") or into an already merged ticket ("It is not possible to
    /// merge into an already merged ticket.") fails with 422.
    /// </para>
    /// </remarks>
    Task<TicketMergeResult> MergeTicketAsync(TicketId sourceTicketId, string targetTicketNumber);

    /// <summary>
    /// Updates several tickets in one transaction, and optionally adds the same article to each.
    /// </summary>
    /// <param name="ticketIds">The tickets to update.</param>
    /// <param name="attributes">
    /// The attributes to set. Blank values (<c>null</c>, empty strings and lists, <c>false</c>) are ignored, so this
    /// can't clear attributes. <see cref="Ticket.Number"/> is ignored.
    /// </param>
    /// <param name="article">An article to add to each ticket, or <c>null</c>.</param>
    /// <remarks>
    /// <para>
    /// If one ticket fails, nothing is changed. Without change access to a ticket, or if saving a ticket or article
    /// fails validation, Zammad answers 422 with <c>{"error": true, "ticket_id": ...}</c>: use
    /// <see cref="ZammadExceptionExtensions.GetFailedTicketId"/> to get the ticket. A ticket that doesn't exist gives
    /// 404.
    /// </para>
    /// <para>
    /// Triggers run for each ticket.
    /// </para>
    /// </remarks>
    Task<TicketMassResult> MassUpdateTicketsAsync(
        IReadOnlyCollection<TicketId> ticketIds,
        Ticket attributes,
        TicketArticle? article = null
    );

    /// <summary>
    /// Applies a macro to several tickets in one transaction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the macro is limited to groups and a ticket is in another group, nothing is changed and Zammad answers 422
    /// "Macro group restrictions do not cover all tickets": use
    /// <see cref="ZammadExceptionExtensions.GetBlockingTicketIds"/> to get those tickets. Failed tickets are reported
    /// like in <see cref="MassUpdateTicketsAsync"/>. A macro or ticket that doesn't exist gives 404.
    /// </para>
    /// <para>
    /// Zammad doesn't check whether the macro is active.
    /// </para>
    /// </remarks>
    Task<TicketMassResult> ApplyMacroToTicketsAsync(MacroId macroId, IReadOnlyCollection<TicketId> ticketIds);
}

public sealed partial class ZammadClient : ITicketService
{
    private const string TicketsEndpoint = "/api/v1/tickets";
    private const string TicketsSearchEndpoint = "/api/v1/tickets/search";
    private const string TicketHistoryEndpoint = "/api/v1/ticket_history";
    private const string TicketMergeEndpoint = "/api/v1/ticket_merge";
    private const string TicketsMassUpdateEndpoint = "/api/v1/tickets/mass_update";
    private const string TicketsMassMacroEndpoint = "/api/v1/tickets/mass_macro";

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

    public async Task<HistoryList?> GetTicketHistoryAsync(TicketId id) =>
        await GetAsync<HistoryList>($"{TicketHistoryEndpoint}/{id}");

    public async Task<TicketMergeResult> MergeTicketAsync(TicketId sourceTicketId, string targetTicketNumber)
    {
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            new Uri(
                _client.BaseAddress!,
                $"{TicketMergeEndpoint}/{sourceTicketId}/{Uri.EscapeDataString(targetTicketNumber)}"
            )
        );

        var httpResponse = await SendAsync(httpRequest);
        var content = await httpResponse.Content.ReadAsStringAsync();
        var response =
            JsonSerializer.Deserialize<TicketMergeResponse>(content) ?? throw LogicException.UnexpectedNullResult;

        if (response.Result != "success")
        {
            // Zammad reports missing tickets with 200 OK
            throw new ZammadException(httpRequest, httpResponse, content, response.Message ?? response.Result);
        }

        return new TicketMergeResult { SourceTicket = response.SourceTicket, TargetTicket = response.TargetTicket };
    }

    public async Task<TicketMassResult> MassUpdateTicketsAsync(
        IReadOnlyCollection<TicketId> ticketIds,
        Ticket attributes,
        TicketArticle? article = null
    ) =>
        await PostAsync<TicketMassResult>(
            TicketsMassUpdateEndpoint,
            new TicketMassUpdateRequest
            {
                TicketIds = ticketIds,
                Attributes = attributes,
                Article = article,
            }
        ) ?? throw LogicException.UnexpectedNullResult;

    public async Task<TicketMassResult> ApplyMacroToTicketsAsync(
        MacroId macroId,
        IReadOnlyCollection<TicketId> ticketIds
    ) =>
        await PostAsync<TicketMassResult>(
            TicketsMassMacroEndpoint,
            new TicketMassMacroRequest { MacroId = macroId, TicketIds = ticketIds }
        ) ?? throw LogicException.UnexpectedNullResult;
}
