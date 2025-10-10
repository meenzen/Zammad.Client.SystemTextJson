using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Resources;

namespace Zammad.Client.Abstractions;

#nullable enable

public interface IGroupService
{
    Task<List<Group>> GetGroupListAsync();
    Task<List<Group>> GetGroupListAsync(int page, int count);
    Task<Group?> GetGroupAsync(int id);
    Task<Group> CreateGroupAsync(Group group);
    Task<Group> UpdateGroupAsync(int id, Group group);
    Task<bool> DeleteGroupAsync(int id);
}
