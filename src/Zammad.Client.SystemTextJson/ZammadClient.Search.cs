using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// The global search across tickets, users, organizations and other models, as in the search bar of the UI.
/// </summary>
/// <remarks>
/// <para>
/// The search goes through Elasticsearch (with a database fallback if it is disabled), so new or changed records
/// show up with a delay. Each model is only searched if the current user may search it, and only records the user
/// can read are returned.
/// </para>
/// <para>
/// <see cref="SearchQuery.Pagination"/> applies to each model separately: with <see cref="Pagination.PerPage"/> 10
/// the result can contain 10 tickets, 10 users and 10 organizations.
/// </para>
/// </remarks>
public interface ISearchService
{
    /// <summary>
    /// Searches the given models, or all models the current user may search if none are given.
    /// </summary>
    Task<SearchResult> SearchAsync(SearchQuery query, params SearchObjectType[] objects);

    /// <summary>
    /// Like <see cref="SearchAsync"/>, but grouped by model and with the total number of hits of each model.
    /// </summary>
    Task<SearchResultByObject> SearchByObjectAsync(SearchQuery query, params SearchObjectType[] objects);
}

public sealed partial class ZammadClient : ISearchService
{
    private const string SearchEndpoint = "/api/v1/search";

    public async Task<SearchResult> SearchAsync(SearchQuery query, params SearchObjectType[] objects) =>
        await GetAsync<SearchResult>(GetSearchPath(objects), GetSearchQuery(query, false).ToString())
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<SearchResultByObject> SearchByObjectAsync(SearchQuery query, params SearchObjectType[] objects) =>
        await GetAsync<SearchResultByObject>(GetSearchPath(objects), GetSearchQuery(query, true).ToString())
        ?? throw LogicException.UnexpectedNullResult;

    private static string GetSearchPath(SearchObjectType[] objects) =>
        objects.Length == 0
            ? SearchEndpoint
            : $"{SearchEndpoint}/{string.Join("-", objects.Distinct().Select(o => o.ToModelName()))}";

    private static QueryBuilder GetSearchQuery(SearchQuery query, bool byObject)
    {
        var builder = new QueryBuilder();
        builder.Add("query", query.Query);
        builder.AddSorting(query.Sorting);
        builder.AddLimitOffsetPagination(query.Pagination);
        if (byObject)
        {
            builder.Add("by_object", true);
        }

        return builder;
    }
}
