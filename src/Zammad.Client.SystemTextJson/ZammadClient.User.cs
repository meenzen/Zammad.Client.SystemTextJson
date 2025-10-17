using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IUserService
{
    Task<User> GetUserMeAsync();
    Task<List<User>> ListUsersAsync();

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<User>> ListUsersAsync(int page, int count);

    Task<List<User>> ListUsersAsync(Pagination? pagination);

    [Obsolete($"Use {nameof(SearchQuery)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<User>> SearchUsersAsync(string query, int limit);

    [Obsolete($"Use {nameof(SearchQuery)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<User>> SearchUsersAsync(string query, int limit, string sortBy, string orderBy);

    Task<List<User>> SearchUsersAsync(SearchQuery query);
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

    public async Task<List<User>> ListUsersAsync() => await GetAsync<List<User>>(UsersEndpoint) ?? [];

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<User>> ListUsersAsync(int page, int count) =>
        await ListUsersAsync(new Pagination { Page = page, PerPage = count });

    public async Task<List<User>> ListUsersAsync(Pagination? pagination)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<User>>(UsersEndpoint, builder.ToString()) ?? [];
    }

    [Obsolete($"Use {nameof(SearchQuery)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<User>> SearchUsersAsync(string query, int limit)
    {
        var builder = new QueryBuilder();
        builder.Add("query", query);
        builder.Add("limit", limit);
        builder.Add("expand", true);
        return await GetAsync<List<User>>(UsersSearchEndpoint, builder.ToString()) ?? [];
    }

    [Obsolete($"Use {nameof(SearchQuery)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<User>> SearchUsersAsync(string query, int limit, string sortBy, string orderBy)
    {
        var builder = new QueryBuilder();
        builder.Add("query", query);
        builder.Add("limit", limit);
        builder.Add("expand", true);
        builder.Add("sort_by", sortBy);
        builder.Add("order_by", orderBy);
        return await GetAsync<List<User>>(UsersSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<User>> SearchUsersAsync(SearchQuery query)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", true);
        return await GetAsync<List<User>>(UsersSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<User?> GetUserAsync(UserId id) => await GetAsync<User>($"{UsersEndpoint}/{id}");

    public async Task<User> CreateUserAsync(User user) =>
        await PostAsync<User>(UsersEndpoint, user) ?? throw LogicException.UnexpectedNullResult;

    public async Task<User> UpdateUserAsync(UserId id, User user) =>
        await PutAsync<User>($"{UsersEndpoint}/{id}", user) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteUserAsync(UserId id) => await DeleteAsync<bool>($"{UsersEndpoint}/{id}");
}
