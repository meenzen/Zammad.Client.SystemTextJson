using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// Roles grant permissions and group access to users.
/// </summary>
/// <remarks>
/// Zammad has no endpoint to delete roles. Set <see cref="Role.Active"/> to <c>false</c> instead.
/// </remarks>
public interface IRoleService
{
    Task<List<Role>> ListRolesAsync(Pagination? pagination = null);
    Task<List<Role>> SearchRolesAsync(SearchQuery query, bool expand = true);
    Task<Role?> GetRoleAsync(RoleId id);

    /// <summary>
    /// Creates a role. Set <see cref="Role.Permissions"/> (names) or <see cref="Role.PermissionIds"/> to grant
    /// permissions.
    /// </summary>
    Task<Role> CreateRoleAsync(Role role);

    /// <summary>
    /// Updates a role. Lists that are set (e.g. <see cref="Role.PermissionIds"/>) replace the current ones.
    /// </summary>
    Task<Role> UpdateRoleAsync(RoleId id, Role role);
}

public sealed partial class ZammadClient : IRoleService
{
    private const string RolesEndpoint = "/api/v1/roles";
    private const string RolesSearchEndpoint = "/api/v1/roles/search";

    public async Task<List<Role>> ListRolesAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Role>>(RolesEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Role>> SearchRolesAsync(SearchQuery query, bool expand = true)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", expand);
        return await GetAsync<List<Role>>(RolesSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Role?> GetRoleAsync(RoleId id) => await GetAsync<Role>($"{RolesEndpoint}/{id}");

    public async Task<Role> CreateRoleAsync(Role role) =>
        await PostAsync<Role>(RolesEndpoint, role) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Role> UpdateRoleAsync(RoleId id, Role role) =>
        await PutAsync<Role>($"{RolesEndpoint}/{id}", role) ?? throw LogicException.UnexpectedNullResult;
}
