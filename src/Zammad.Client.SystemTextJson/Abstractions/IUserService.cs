using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Resources;

namespace Zammad.Client.Abstractions;

#nullable enable
public interface IUserService
{
    Task<User> GetUserMeAsync();
    Task<List<User>> GetUserListAsync();
    Task<List<User>> GetUserListAsync(int page, int count);
    Task<List<User>> SearchUserAsync(string query, int limit);
    Task<List<User>> SearchUserAsync(string query, int limit, string sortBy, string orderBy);
    Task<User?> GetUserAsync(int id);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(int id, User user);
    Task<bool> DeleteUserAsync(int id);
}
