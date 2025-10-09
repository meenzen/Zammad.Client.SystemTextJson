using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Services;

namespace Zammad.Client;

public class UserClient : ZammadClient, IUserService
{
    public UserClient(ZammadAccount account)
        : base(account) { }

    #region IUserService

    public Task<User> GetUserMeAsync() => GetAsync<User>("/api/v1/users/me");

    public Task<IList<User>> GetUserListAsync() => GetAsync<IList<User>>("/api/v1/users");

    public Task<IList<User>> GetUserListAsync(int page, int count) =>
        GetAsync<IList<User>>("/api/v1/users", $"page={page}&per_page={count}");

    public Task<IList<User>> SearchUserAsync(string query, int limit) =>
        GetAsync<IList<User>>("/api/v1/users/search", $"query={query}&limit={limit}&expand=true");

    public Task<IList<User>> SearchUserAsync(string query, int limit, string sortBy, string orderBy) =>
        GetAsync<IList<User>>(
            "/api/v1/users/search",
            $"query={query}&limit={limit}&expand=true&sort_by={sortBy}&order_by={orderBy}"
        );

    public Task<User> GetUserAsync(int id) => GetAsync<User>($"/api/v1/users/{id}");

    public Task<User> CreateUserAsync(User user) => PostAsync<User>("/api/v1/users", user);

    public Task<User> UpdateUserAsync(int id, User user) => PutAsync<User>($"/api/v1/users/{id}", user);

    public Task<bool> DeleteUserAsync(int id) => DeleteAsync<bool>($"/api/v1/users/{id}");

    #endregion
}
