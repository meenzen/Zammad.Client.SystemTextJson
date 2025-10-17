using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IGroupService
{
    Task<List<Group>> ListGroupsAsync();

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<Group>> ListGroupsAsync(int page, int count);

    Task<List<Group>> ListGroupsAsync(Pagination? pagination);
    Task<Group?> GetGroupAsync(GroupId id);
    Task<Group> CreateGroupAsync(Group group);
    Task<Group> UpdateGroupAsync(GroupId id, Group group);
    Task<bool> DeleteGroupAsync(GroupId id);
}

public sealed partial class ZammadClient : IGroupService
{
    private const string GroupsEndpoint = "/api/v1/groups";

    public async Task<List<Group>> ListGroupsAsync() => await GetAsync<List<Group>>(GroupsEndpoint) ?? [];

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<Group>> ListGroupsAsync(int page, int count) =>
        await ListGroupsAsync(new Pagination { Page = page, PerPage = count });

    public async Task<List<Group>> ListGroupsAsync(Pagination? pagination)
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
