using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IOrganizationService
{
    Task<List<Organization>> ListOrganizationsAsync();

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<Organization>> ListOrganizationsAsync(int page, int count);

    Task<List<Organization>> ListOrganizationsAsync(Pagination? pagination);

    [Obsolete($"Use {nameof(SearchQuery)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<Organization>> SearchOrganizationsAsync(string query, int limit);

    [Obsolete($"Use {nameof(SearchQuery)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<Organization>> SearchOrganizationsAsync(string query, int limit, string sortBy, string orderBy);

    Task<List<Organization>> SearchOrganizationsAsync(SearchQuery query);
    Task<Organization?> GetOrganizationAsync(OrganizationId id);
    Task<Organization> CreateOrganizationAsync(Organization organization);
    Task<Organization> UpdateOrganizationAsync(OrganizationId id, Organization organization);
    Task<bool> DeleteOrganizationAsync(OrganizationId id);
}

public sealed partial class ZammadClient : IOrganizationService
{
    private const string OrganizationsEndpoint = "/api/v1/organizations";
    private const string OrganizationsSearchEndpoint = "/api/v1/organizations/search";

    public async Task<List<Organization>> ListOrganizationsAsync() =>
        await GetAsync<List<Organization>>(OrganizationsEndpoint) ?? [];

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<Organization>> ListOrganizationsAsync(int page, int count) =>
        await ListOrganizationsAsync(new Pagination { Page = page, PerPage = count });

    public async Task<List<Organization>> ListOrganizationsAsync(Pagination? pagination)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Organization>>(OrganizationsEndpoint, builder.ToString()) ?? [];
    }

    [Obsolete($"Use {nameof(SearchQuery)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<Organization>> SearchOrganizationsAsync(string query, int limit)
    {
        var builder = new QueryBuilder();
        builder.Add("query", query);
        builder.Add("limit", limit);
        builder.Add("expand", true);
        return await GetAsync<List<Organization>>(OrganizationsSearchEndpoint, builder.ToString()) ?? [];
    }

    [Obsolete($"Use {nameof(SearchQuery)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<Organization>> SearchOrganizationsAsync(
        string query,
        int limit,
        string sortBy,
        string orderBy
    )
    {
        var builder = new QueryBuilder();
        builder.Add("query", query);
        builder.Add("limit", limit);
        builder.Add("expand", true);
        builder.Add("sort_by", sortBy);
        builder.Add("order_by", orderBy);
        return await GetAsync<List<Organization>>(OrganizationsSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Organization>> SearchOrganizationsAsync(SearchQuery query)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", true);
        return await GetAsync<List<Organization>>(OrganizationsSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Organization?> GetOrganizationAsync(OrganizationId id) =>
        await GetAsync<Organization>($"{OrganizationsEndpoint}/{id}");

    public async Task<Organization> CreateOrganizationAsync(Organization organization) =>
        await PostAsync<Organization>(OrganizationsEndpoint, organization) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Organization> UpdateOrganizationAsync(OrganizationId id, Organization organization) =>
        await PutAsync<Organization>($"{OrganizationsEndpoint}/{id}", organization)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteOrganizationAsync(OrganizationId id) =>
        await DeleteAsync<bool>($"{OrganizationsEndpoint}/{id}");
}
