using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

/// <summary>
/// Manages the overview definitions. All endpoints need the <c>admin.overview</c> permission.
/// </summary>
public interface IOverviewService
{
    Task<List<Overview>> ListOverviewsAsync(Pagination? pagination = null);

    /// <summary>
    /// Searches overviews.
    /// </summary>
    /// <remarks>
    /// The search goes through Elasticsearch, so new or changed overviews show up with a short delay.
    /// </remarks>
    Task<List<Overview>> SearchOverviewsAsync(SearchQuery query, bool expand = true);

    Task<Overview?> GetOverviewAsync(OverviewId id);

    /// <summary>
    /// Creates an overview.
    /// </summary>
    /// <remarks>
    /// <see cref="Overview.RoleIds"/> must not be empty, and <see cref="Overview.Condition"/> must be a valid ticket
    /// selector. If <see cref="Overview.Link"/> is empty, Zammad derives it from the name, and it appends a suffix
    /// (<c>_1</c>, <c>_2</c>, …) if the link is already taken. Without <see cref="Overview.Prio"/>, the overview is
    /// added at the end.
    /// </remarks>
    Task<Overview> CreateOverviewAsync(Overview overview);

    /// <summary>
    /// Updates an overview.
    /// </summary>
    /// <remarks>
    /// Changing <see cref="Overview.Prio"/> renumbers the other overviews to make room for the new position.
    /// </remarks>
    Task<Overview> UpdateOverviewAsync(OverviewId id, Overview overview);

    Task DeleteOverviewAsync(OverviewId id);

    /// <summary>
    /// Sets <see cref="Overview.Prio"/> of the given overviews (<c>POST /api/v1/overviews_prio</c>). Overviews
    /// are sorted by ascending prio.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="UpdateOverviewAsync"/>, this doesn't renumber the overviews that aren't listed, so
    /// several overviews can end up with the same prio. Unknown IDs make the request fail with a 500 error.
    /// </remarks>
    Task SetOverviewPrioritiesAsync(IReadOnlyDictionary<OverviewId, int> priorities);
}

public sealed partial class ZammadClient : IOverviewService
{
    private const string OverviewsEndpoint = "/api/v1/overviews";
    private const string OverviewsSearchEndpoint = "/api/v1/overviews/search";
    private const string OverviewsPrioEndpoint = "/api/v1/overviews_prio";

    public async Task<List<Overview>> ListOverviewsAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Overview>>(OverviewsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Overview>> SearchOverviewsAsync(SearchQuery query, bool expand = true)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", expand);
        return await GetAsync<List<Overview>>(OverviewsSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Overview?> GetOverviewAsync(OverviewId id) =>
        await GetAsync<Overview>($"{OverviewsEndpoint}/{id}");

    public async Task<Overview> CreateOverviewAsync(Overview overview) =>
        await PostAsync<Overview>(OverviewsEndpoint, overview) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Overview> UpdateOverviewAsync(OverviewId id, Overview overview) =>
        await PutAsync<Overview>($"{OverviewsEndpoint}/{id}", overview) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteOverviewAsync(OverviewId id) => await DeleteAsync<bool>($"{OverviewsEndpoint}/{id}");

    public async Task SetOverviewPrioritiesAsync(IReadOnlyDictionary<OverviewId, int> priorities) =>
        await PostAsync<bool>(OverviewsPrioEndpoint, OverviewPrioRequest.Create(priorities));
}
