using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IUserService
{
    Task<User> GetUserMeAsync();
    Task<List<User>> ListUsersAsync();
    Task<List<User>> ListUsersAsync(int page, int count);
    Task<List<User>> SearchUsersAsync(string query, int limit);
    Task<List<User>> SearchUsersAsync(string query, int limit, string sortBy, string orderBy);
    Task<User?> GetUserAsync(UserId id);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(UserId id, User user);
    Task<bool> DeleteUserAsync(UserId id);
}

public sealed partial class ZammadClient : IUserService
{
    public async Task<User> GetUserMeAsync() =>
        await GetAsync<User>("/api/v1/users/me") ?? throw LogicException.UnexpectedNullResult;

    public async Task<List<User>> ListUsersAsync() => await GetAsync<List<User>>("/api/v1/users") ?? [];

    public async Task<List<User>> ListUsersAsync(int page, int count)
    {
        var builder = new QueryBuilder();
        builder.Add("page", page);
        builder.Add("per_page", count);
        return await GetAsync<List<User>>("/api/v1/users", builder.ToString()) ?? [];
    }

    public async Task<List<User>> SearchUsersAsync(string query, int limit)
    {
        var builder = new QueryBuilder();
        builder.Add("query", query);
        builder.Add("limit", limit);
        builder.Add("expand", true);
        return await GetAsync<List<User>>("/api/v1/users/search", builder.ToString()) ?? [];
    }

    public async Task<List<User>> SearchUsersAsync(string query, int limit, string sortBy, string orderBy)
    {
        var builder = new QueryBuilder();
        builder.Add("query", query);
        builder.Add("limit", limit);
        builder.Add("expand", true);
        builder.Add("sort_by", sortBy);
        builder.Add("order_by", orderBy);
        return await GetAsync<List<User>>("/api/v1/users/search", builder.ToString()) ?? [];
    }

    public async Task<User?> GetUserAsync(UserId id) => await GetAsync<User>($"/api/v1/users/{id}");

    public async Task<User> CreateUserAsync(User user) =>
        await PostAsync<User>("/api/v1/users", user) ?? throw LogicException.UnexpectedNullResult;

    public async Task<User> UpdateUserAsync(UserId id, User user) =>
        await PutAsync<User>($"/api/v1/users/{id}", user) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteUserAsync(UserId id) => await DeleteAsync<bool>($"/api/v1/users/{id}");
}
