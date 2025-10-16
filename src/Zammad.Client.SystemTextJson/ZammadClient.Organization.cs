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
    public async Task<List<Organization>> ListOrganizationsAsync() =>
        await GetAsync<List<Organization>>("/api/v1/organizations") ?? [];

    public async Task<List<Organization>> ListOrganizationsAsync(int page, int count) =>
        await GetAsync<List<Organization>>("/api/v1/organizations", $"page={page}&per_page={count}") ?? [];

    public async Task<List<Organization>> SearchOrganizationsAsync(string query, int limit) =>
        await GetAsync<List<Organization>>("/api/v1/organizations/search", $"query={query}&limit={limit}&expand=true")
        ?? [];

    public async Task<List<Organization>> SearchOrganizationsAsync(
        string query,
        int limit,
        string sortBy,
        string orderBy
    ) =>
        await GetAsync<List<Organization>>(
            "/api/v1/organizations/search",
            $"query={query}&limit={limit}&expand=true&sort_by={sortBy}&order_by={orderBy}"
        ) ?? [];

    public async Task<Organization?> GetOrganizationAsync(OrganizationId id) =>
        await GetAsync<Organization>($"/api/v1/organizations/{id}");

    public async Task<Organization> CreateOrganizationAsync(Organization organization) =>
        await PostAsync<Organization>("/api/v1/organizations", organization)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<Organization> UpdateOrganizationAsync(OrganizationId id, Organization organization) =>
        await PutAsync<Organization>($"/api/v1/organizations/{id}", organization)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteOrganizationAsync(OrganizationId id) =>
        await DeleteAsync<bool>($"/api/v1/organizations/{id}");
}
