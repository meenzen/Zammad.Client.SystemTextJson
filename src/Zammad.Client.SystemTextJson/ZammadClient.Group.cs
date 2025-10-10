using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Abstractions;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public sealed partial class ZammadClient : IGroupService
{
    public Task<List<Group>> GetGroupListAsync() => GetAsync<List<Group>>("/api/v1/groups");

    public Task<List<Group>> GetGroupListAsync(int page, int count) =>
        GetAsync<List<Group>>("/api/v1/groups", $"page={page}&per_page={count}");

    public Task<Group> GetGroupAsync(int id) => GetAsync<Group>($"/api/v1/groups/{id}");

    public Task<Group> CreateGroupAsync(Group group) => PostAsync<Group>("/api/v1/groups", group);

    public Task<Group> UpdateGroupAsync(int id, Group group) => PutAsync<Group>($"/api/v1/groups/{id}", group);

    public Task<bool> DeleteGroupAsync(int id) => DeleteAsync<bool>($"/api/v1/groups/{id}");
}
