using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// Postmaster filters (<c>/api/v1/postmaster_filters</c>), which change incoming emails before Zammad creates or
/// updates tickets from them. Requires one of the <c>admin.channel_email</c>, <c>admin.channel_google</c>,
/// <c>admin.channel_microsoft365</c> or <c>admin.channel_microsoft_graph</c> permissions.
/// </summary>
public interface IPostmasterFilterService
{
    Task<List<PostmasterFilter>> ListPostmasterFiltersAsync(Pagination? pagination = null);

    Task<PostmasterFilter?> GetPostmasterFilterAsync(PostmasterFilterId id);

    /// <summary>
    /// Creates a postmaster filter.
    /// </summary>
    /// <remarks>
    /// <see cref="PostmasterFilter.Name"/>, <see cref="PostmasterFilter.Channel"/> (<c>email</c>) and at least one
    /// <see cref="PostmasterFilter.Match"/> rule are required. Every match rule needs an <c>operator</c> (e.g.
    /// <c>contains</c>, <c>matches regex</c>) and a <c>value</c>, otherwise Zammad responds with 422.
    /// </remarks>
    Task<PostmasterFilter> CreatePostmasterFilterAsync(PostmasterFilter postmasterFilter);

    Task<PostmasterFilter> UpdatePostmasterFilterAsync(PostmasterFilterId id, PostmasterFilter postmasterFilter);

    Task DeletePostmasterFilterAsync(PostmasterFilterId id);
}

public sealed partial class ZammadClient : IPostmasterFilterService
{
    private const string PostmasterFiltersEndpoint = "/api/v1/postmaster_filters";

    public async Task<List<PostmasterFilter>> ListPostmasterFiltersAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<PostmasterFilter>>(PostmasterFiltersEndpoint, builder.ToString()) ?? [];
    }

    public async Task<PostmasterFilter?> GetPostmasterFilterAsync(PostmasterFilterId id) =>
        await GetAsync<PostmasterFilter>($"{PostmasterFiltersEndpoint}/{id}");

    public async Task<PostmasterFilter> CreatePostmasterFilterAsync(PostmasterFilter postmasterFilter) =>
        await PostAsync<PostmasterFilter>(PostmasterFiltersEndpoint, postmasterFilter)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<PostmasterFilter> UpdatePostmasterFilterAsync(
        PostmasterFilterId id,
        PostmasterFilter postmasterFilter
    ) =>
        await PutAsync<PostmasterFilter>($"{PostmasterFiltersEndpoint}/{id}", postmasterFilter)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task DeletePostmasterFilterAsync(PostmasterFilterId id) =>
        await DeleteAsync<bool>($"{PostmasterFiltersEndpoint}/{id}");
}
