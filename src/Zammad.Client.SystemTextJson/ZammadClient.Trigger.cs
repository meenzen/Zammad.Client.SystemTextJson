using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// Triggers (<c>/api/v1/triggers</c>). Requires the <c>admin.trigger</c> permission.
/// </summary>
public interface ITriggerService
{
    Task<List<Trigger>> ListTriggersAsync(Pagination? pagination = null);

    /// <summary>
    /// Searches triggers by name and note.
    /// </summary>
    /// <remarks>
    /// With a query, Zammad searches the Elasticsearch index, so new or changed triggers show up with a delay.
    /// </remarks>
    Task<List<Trigger>> SearchTriggersAsync(SearchQuery query);

    Task<Trigger?> GetTriggerAsync(TriggerId id);

    /// <summary>
    /// Creates a trigger.
    /// </summary>
    /// <remarks>
    /// <see cref="Trigger.Name"/> (unique, case-insensitive), <see cref="Trigger.Condition"/> and
    /// <see cref="Trigger.Perform"/> are required. The condition must be a valid ticket selector, otherwise Zammad
    /// responds with 422 "Invalid object selector conditions".
    /// Triggers are <see cref="Trigger.Active"/> by default and then run on every matching ticket change.
    /// </remarks>
    Task<Trigger> CreateTriggerAsync(Trigger trigger);

    Task<Trigger> UpdateTriggerAsync(TriggerId id, Trigger trigger);

    Task DeleteTriggerAsync(TriggerId id);
}

public sealed partial class ZammadClient : ITriggerService
{
    private const string TriggersEndpoint = "/api/v1/triggers";
    private const string TriggersSearchEndpoint = "/api/v1/triggers/search";

    public async Task<List<Trigger>> ListTriggersAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Trigger>>(TriggersEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Trigger>> SearchTriggersAsync(SearchQuery query)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        return await GetAsync<List<Trigger>>(TriggersSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Trigger?> GetTriggerAsync(TriggerId id) => await GetAsync<Trigger>($"{TriggersEndpoint}/{id}");

    public async Task<Trigger> CreateTriggerAsync(Trigger trigger) =>
        await PostAsync<Trigger>(TriggersEndpoint, trigger) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Trigger> UpdateTriggerAsync(TriggerId id, Trigger trigger) =>
        await PutAsync<Trigger>($"{TriggersEndpoint}/{id}", trigger) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteTriggerAsync(TriggerId id) => await DeleteAsync<bool>($"{TriggersEndpoint}/{id}");
}
