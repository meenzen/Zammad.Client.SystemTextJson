using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IGroupService
{
    Task<List<Group>> ListGroupsAsync(Pagination? pagination = null);
    Task<Group?> GetGroupAsync(GroupId id);
    Task<Group> CreateGroupAsync(Group group);
    Task<Group> UpdateGroupAsync(GroupId id, Group group);
    Task<bool> DeleteGroupAsync(GroupId id);
}

public sealed partial class ZammadClient : IGroupService
{
    private const string GroupsEndpoint = "/api/v1/groups";

    public async Task<List<Group>> ListGroupsAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Group>>(GroupsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Group?> GetGroupAsync(GroupId id) => await GetAsync<Group>($"{GroupsEndpoint}/{id}");

    public async Task<Group> CreateGroupAsync(Group group) =>
        await PostAsync<Group>(GroupsEndpoint, group) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Group> UpdateGroupAsync(GroupId id, Group group) =>
        await PutAsync<Group>($"{GroupsEndpoint}/{id}", group) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteGroupAsync(GroupId id) => await DeleteAsync<bool>($"{GroupsEndpoint}/{id}");
}
