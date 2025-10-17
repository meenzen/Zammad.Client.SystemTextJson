using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IOrganizationService
{
    Task<List<Organization>> ListOrganizationsAsync();
    Task<List<Organization>> ListOrganizationsAsync(int page, int count);
    Task<List<Organization>> SearchOrganizationsAsync(string query, int limit);
    Task<List<Organization>> SearchOrganizationsAsync(string query, int limit, string sortBy, string orderBy);
    Task<Organization?> GetOrganizationAsync(OrganizationId id);
    Task<Organization> CreateOrganizationAsync(Organization organization);
    Task<Organization> UpdateOrganizationAsync(OrganizationId id, Organization organization);
    Task<bool> DeleteOrganizationAsync(OrganizationId id);
}

public sealed partial class ZammadClient : IOrganizationService
{
    private const string OrganizationsEndpoint = "/api/v1/organizations";

    public async Task<List<Organization>> ListOrganizationsAsync() =>
        await GetAsync<List<Organization>>(OrganizationsEndpoint) ?? [];

    public async Task<List<Organization>> ListOrganizationsAsync(int page, int count)
    {
        var builder = new QueryBuilder();
        builder.Add("page", page);
        builder.Add("per_page", count);
        return await GetAsync<List<Organization>>(OrganizationsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Organization>> SearchOrganizationsAsync(string query, int limit)
    {
        var builder = new QueryBuilder();
        builder.Add("query", query);
        builder.Add("limit", limit);
        builder.Add("expand", true);
        return await GetAsync<List<Organization>>($"{OrganizationsEndpoint}/search", builder.ToString()) ?? [];
    }

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
        return await GetAsync<List<Organization>>($"{OrganizationsEndpoint}/search", builder.ToString()) ?? [];
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
