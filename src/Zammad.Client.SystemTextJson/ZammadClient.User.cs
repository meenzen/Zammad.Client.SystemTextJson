using System.Diagnostics.CodeAnalysis;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IUserService
{
    Task<User> GetUserMeAsync();
    Task<List<User>> ListUsersAsync(Pagination? pagination = null);
    Task<List<User>> SearchUsersAsync(SearchQuery query, bool expand = true);
    Task<User?> GetUserAsync(UserId id);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(UserId id, User user);
    Task<bool> DeleteUserAsync(UserId id);
}

public sealed partial class ZammadClient : IUserService
{
    private const string UsersEndpoint = "/api/v1/users";
    private const string UsersSearchEndpoint = "/api/v1/users/search";

    public async Task<User> GetUserMeAsync() =>
        await GetAsync<User>($"{UsersEndpoint}/me") ?? throw LogicException.UnexpectedNullResult;

    public async Task<List<User>> ListUsersAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<User>>(UsersEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<User>> SearchUsersAsync(SearchQuery query, bool expand = true)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", expand);
        return await GetAsync<List<User>>(UsersSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<User?> GetUserAsync(UserId id) => await GetAsync<User>($"{UsersEndpoint}/{id}");

    public async Task<User> CreateUserAsync(User user) =>
        await PostAsync<User>(UsersEndpoint, user) ?? throw LogicException.UnexpectedNullResult;

    public async Task<User> UpdateUserAsync(UserId id, User user) =>
        await PutAsync<User>($"{UsersEndpoint}/{id}", user) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteUserAsync(UserId id) => await DeleteAsync<bool>($"{UsersEndpoint}/{id}");
}
