using System.Diagnostics.CodeAnalysis;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IOrganizationService
{
    Task<List<Organization>> ListOrganizationsAsync(Pagination? pagination = null);
    Task<List<Organization>> SearchOrganizationsAsync(SearchQuery query, bool expand = true);
    Task<Organization?> GetOrganizationAsync(OrganizationId id);
    Task<Organization> CreateOrganizationAsync(Organization organization);
    Task<Organization> UpdateOrganizationAsync(OrganizationId id, Organization organization);
    Task<bool> DeleteOrganizationAsync(OrganizationId id);
}

public sealed partial class ZammadClient : IOrganizationService
{
    private const string OrganizationsEndpoint = "/api/v1/organizations";
    private const string OrganizationsSearchEndpoint = "/api/v1/organizations/search";

    public async Task<List<Organization>> ListOrganizationsAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Organization>>(OrganizationsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Organization>> SearchOrganizationsAsync(SearchQuery query, bool expand = true)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", expand);
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
