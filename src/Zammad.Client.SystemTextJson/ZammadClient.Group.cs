using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IGroupService
{
    Task<List<Group>> GetGroupListAsync();
    Task<List<Group>> GetGroupListAsync(int page, int count);
    Task<Group?> GetGroupAsync(int id);
    Task<Group> CreateGroupAsync(Group group);
    Task<Group> UpdateGroupAsync(int id, Group group);
    Task<bool> DeleteGroupAsync(int id);
}

public sealed partial class ZammadClient : IGroupService
{
    public async Task<List<Group>> GetGroupListAsync() => await GetAsync<List<Group>>("/api/v1/groups") ?? [];

    public async Task<List<Group>> GetGroupListAsync(int page, int count) =>
        await GetAsync<List<Group>>("/api/v1/groups", $"page={page}&per_page={count}") ?? [];

    public async Task<Group?> GetGroupAsync(int id) => await GetAsync<Group>($"/api/v1/groups/{id}");

    public async Task<Group> CreateGroupAsync(Group group) =>
        await PostAsync<Group>("/api/v1/groups", group) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Group> UpdateGroupAsync(int id, Group group) =>
        await PutAsync<Group>($"/api/v1/groups/{id}", group) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteGroupAsync(int id) => await DeleteAsync<bool>($"/api/v1/groups/{id}");
}
