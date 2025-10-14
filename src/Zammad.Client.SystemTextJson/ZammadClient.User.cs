using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IUserService
{
    Task<User> GetUserMeAsync();
    Task<List<User>> GetUserListAsync();
    Task<List<User>> GetUserListAsync(int page, int count);
    Task<List<User>> SearchUserAsync(string query, int limit);
    Task<List<User>> SearchUserAsync(string query, int limit, string sortBy, string orderBy);
    Task<User?> GetUserAsync(UserId id);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(UserId id, User user);
    Task<bool> DeleteUserAsync(UserId id);
}

public sealed partial class ZammadClient : IUserService
{
    public async Task<User> GetUserMeAsync() =>
        await GetAsync<User>("/api/v1/users/me") ?? throw LogicException.UnexpectedNullResult;

    public async Task<List<User>> GetUserListAsync() => await GetAsync<List<User>>("/api/v1/users") ?? [];

    public async Task<List<User>> GetUserListAsync(int page, int count) =>
        await GetAsync<List<User>>("/api/v1/users", $"page={page}&per_page={count}") ?? [];

    public async Task<List<User>> SearchUserAsync(string query, int limit) =>
        await GetAsync<List<User>>("/api/v1/users/search", $"query={query}&limit={limit}&expand=true") ?? [];

    public async Task<List<User>> SearchUserAsync(string query, int limit, string sortBy, string orderBy) =>
        await GetAsync<List<User>>(
            "/api/v1/users/search",
            $"query={query}&limit={limit}&expand=true&sort_by={sortBy}&order_by={orderBy}"
        ) ?? [];

    public async Task<User?> GetUserAsync(UserId id) => await GetAsync<User>($"/api/v1/users/{id}");

    public async Task<User> CreateUserAsync(User user) =>
        await PostAsync<User>("/api/v1/users", user) ?? throw LogicException.UnexpectedNullResult;

    public async Task<User> UpdateUserAsync(UserId id, User user) =>
        await PutAsync<User>($"/api/v1/users/{id}", user) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteUserAsync(UserId id) => await DeleteAsync<bool>($"/api/v1/users/{id}");
}
