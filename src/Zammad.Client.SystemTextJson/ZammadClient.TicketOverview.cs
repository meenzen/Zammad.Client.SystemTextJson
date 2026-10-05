using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// The ticket overviews of the current user, as shown in the agent UI. To manage overviews, use
/// <see cref="IOverviewService"/>.
/// </summary>
/// <remarks>
/// Overviews are only available to users whose roles the overview is assigned to (agents and customers). Tickets in
/// an overview are counted and listed with the current user's permissions.
/// </remarks>
public interface ITicketOverviewService
{
    /// <summary>
    /// Lists the overviews of the current user with their ticket counts, ordered by prio.
    /// </summary>
    Task<List<TicketOverviewSummary>> ListTicketOverviewsAsync();

    /// <summary>
    /// Gets the tickets of an overview.
    /// </summary>
    /// <param name="link">The <see cref="TicketOverviewSummary.Link"/> of the overview, e.g. <c>my_assigned</c>.</param>
    /// <returns>
    /// The tickets, or <c>null</c> if the current user has no overview with this link.
    /// </returns>
    /// <remarks>
    /// The tickets are sorted like in the UI and limited by the <c>ui_ticket_overview_ticket_limit</c> setting
    /// (default 2000), there is no paging. <see cref="TicketOverviewIndex.Count"/> is the number of all tickets.
    /// Zammad computes all overviews of the user for this request.
    /// </remarks>
    Task<TicketOverviewResult?> GetTicketOverviewAsync(string link);
}

public sealed partial class ZammadClient : ITicketOverviewService
{
    private const string TicketOverviewsEndpoint = "/api/v1/ticket_overviews";

    public async Task<List<TicketOverviewSummary>> ListTicketOverviewsAsync() =>
        await GetAsync<List<TicketOverviewSummary>>(TicketOverviewsEndpoint) ?? [];

    public async Task<TicketOverviewResult?> GetTicketOverviewAsync(string link)
    {
        var builder = new QueryBuilder();
        builder.Add("view", link);
        var result = await GetAsync<TicketOverviewResult>(TicketOverviewsEndpoint, builder.ToString());

        // Zammad answers {"assets": {}, "index": {}} for unknown links
        return result?.Index.Overview is null ? null : result;
    }
}
