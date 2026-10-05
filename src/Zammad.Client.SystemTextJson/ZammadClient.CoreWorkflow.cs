using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// Core workflows (<c>/api/v1/core_workflows</c>), which change form fields (visibility, mandatory, options) based
/// on conditions. Requires the <c>admin.core_workflow</c> permission.
/// </summary>
/// <remarks>
/// Only workflows with <see cref="CoreWorkflow.Changeable"/> set are visible through this API. Zammad's built-in
/// workflows aren't changeable, so they are neither listed nor returned by
/// <see cref="GetCoreWorkflowAsync"/>. <c>POST /api/v1/core_workflows/perform</c> isn't supported: it evaluates
/// workflows for a form in Zammad's UI.
/// </remarks>
public interface ICoreWorkflowService
{
    Task<List<CoreWorkflow>> ListCoreWorkflowsAsync(Pagination? pagination = null);

    /// <summary>
    /// Searches changeable core workflows by name.
    /// </summary>
    /// <remarks>
    /// With a query, Zammad searches the Elasticsearch index, so new or changed workflows show up with a delay.
    /// </remarks>
    Task<List<CoreWorkflow>> SearchCoreWorkflowsAsync(SearchQuery query);

    Task<CoreWorkflow?> GetCoreWorkflowAsync(CoreWorkflowId id);

    /// <summary>
    /// Creates a core workflow.
    /// </summary>
    /// <remarks>
    /// <see cref="CoreWorkflow.Name"/> is required and must be unique (case-insensitive). Zammad doesn't validate
    /// <see cref="CoreWorkflow.Object"/> through the API; use <c>Ticket</c>, <c>User</c>, <c>Organization</c> or
    /// <c>Group</c>. Workflows are <see cref="CoreWorkflow.Active"/> by default, and active workflows also apply to
    /// API requests: Zammad validates creates and updates of the object against them (e.g. mandatory fields).
    /// </remarks>
    Task<CoreWorkflow> CreateCoreWorkflowAsync(CoreWorkflow coreWorkflow);

    Task<CoreWorkflow> UpdateCoreWorkflowAsync(CoreWorkflowId id, CoreWorkflow coreWorkflow);

    Task DeleteCoreWorkflowAsync(CoreWorkflowId id);
}

public sealed partial class ZammadClient : ICoreWorkflowService
{
    private const string CoreWorkflowsEndpoint = "/api/v1/core_workflows";
    private const string CoreWorkflowsSearchEndpoint = "/api/v1/core_workflows/search";

    public async Task<List<CoreWorkflow>> ListCoreWorkflowsAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<CoreWorkflow>>(CoreWorkflowsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<CoreWorkflow>> SearchCoreWorkflowsAsync(SearchQuery query)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        return await GetAsync<List<CoreWorkflow>>(CoreWorkflowsSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<CoreWorkflow?> GetCoreWorkflowAsync(CoreWorkflowId id) =>
        await GetAsync<CoreWorkflow>($"{CoreWorkflowsEndpoint}/{id}");

    public async Task<CoreWorkflow> CreateCoreWorkflowAsync(CoreWorkflow coreWorkflow) =>
        await PostAsync<CoreWorkflow>(CoreWorkflowsEndpoint, coreWorkflow) ?? throw LogicException.UnexpectedNullResult;

    public async Task<CoreWorkflow> UpdateCoreWorkflowAsync(CoreWorkflowId id, CoreWorkflow coreWorkflow) =>
        await PutAsync<CoreWorkflow>($"{CoreWorkflowsEndpoint}/{id}", coreWorkflow)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteCoreWorkflowAsync(CoreWorkflowId id) =>
        await DeleteAsync<bool>($"{CoreWorkflowsEndpoint}/{id}");
}
