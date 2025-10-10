using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Abstractions;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public sealed partial class ZammadClient : IOrganizationService
{
    public Task<List<Organization>> GetOrganizationListAsync() => GetAsync<List<Organization>>("/api/v1/organizations");

    public Task<List<Organization>> GetOrganizationListAsync(int page, int count) =>
        GetAsync<List<Organization>>("/api/v1/organizations", $"page={page}&per_page={count}");

    public Task<List<Organization>> SearchOrganizationAsync(string query, int limit) =>
        GetAsync<List<Organization>>("/api/v1/organizations/search", $"query={query}&limit={limit}&expand=true");

    public Task<List<Organization>> SearchOrganizationAsync(string query, int limit, string sortBy, string orderBy) =>
        GetAsync<List<Organization>>(
            "/api/v1/organizations/search",
            $"query={query}&limit={limit}&expand=true&sort_by={sortBy}&order_by={orderBy}"
        );

    public Task<Organization> GetOrganizationAsync(int id) => GetAsync<Organization>($"/api/v1/organizations/{id}");

    public Task<Organization> CreateOrganizationAsync(Organization organization) =>
        PostAsync<Organization>("/api/v1/organizations", organization);

    public Task<Organization> UpdateOrganizationAsync(int id, Organization organization) =>
        PutAsync<Organization>($"/api/v1/organizations/{id}", organization);

    public Task<bool> DeleteOrganizationAsync(int id) => DeleteAsync<bool>($"/api/v1/organizations/{id}");
}
