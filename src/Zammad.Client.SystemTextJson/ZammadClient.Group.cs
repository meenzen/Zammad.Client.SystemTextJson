using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IGroupService
{
    Task<List<Group>> ListGroupsAsync(Pagination? pagination = null);
    Task<List<Group>> SearchGroupsAsync(SearchQuery query, bool expand = true);
    Task<Group?> GetGroupAsync(GroupId id);
    Task<Group> CreateGroupAsync(Group group);
    Task<Group> UpdateGroupAsync(GroupId id, Group group);
    Task DeleteGroupAsync(GroupId id);
}

public sealed partial class ZammadClient : IGroupService
{
    private const string GroupsEndpoint = "/api/v1/groups";
    private const string GroupsSearchEndpoint = "/api/v1/groups/search";

    public async Task<List<Group>> ListGroupsAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Group>>(GroupsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Group>> SearchGroupsAsync(SearchQuery query, bool expand = true)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", expand);
        return await GetAsync<List<Group>>(GroupsSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Group?> GetGroupAsync(GroupId id) => await GetAsync<Group>($"{GroupsEndpoint}/{id}");

    public async Task<Group> CreateGroupAsync(Group group) =>
        await PostAsync<Group>(GroupsEndpoint, group) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Group> UpdateGroupAsync(GroupId id, Group group) =>
        await PutAsync<Group>($"{GroupsEndpoint}/{id}", group) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteGroupAsync(GroupId id) => await DeleteAsync<bool>($"{GroupsEndpoint}/{id}");
}
