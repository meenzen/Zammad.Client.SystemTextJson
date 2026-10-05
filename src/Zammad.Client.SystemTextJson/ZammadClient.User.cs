using System.Diagnostics.CodeAnalysis;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public interface IUserService
{
    Task<User> GetUserMeAsync();
    Task<List<User>> ListUsersAsync(Pagination? pagination = null);
    Task<List<User>> SearchUsersAsync(SearchQuery query, bool expand = true);
    Task<User?> GetUserAsync(UserId id);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(UserId id, User user);
    Task DeleteUserAsync(UserId id);

    /// <summary>
    /// Resets the failed login counter of a user who is locked out after too many failed logins.
    /// </summary>
    Task UnlockUserAsync(UserId id);

    /// <summary>
    /// Lists the two-factor methods that are enabled in the system settings, and whether the user has configured them.
    /// </summary>
    Task<List<TwoFactorMethod>> ListUserTwoFactorMethodsAsync(UserId id);

    /// <summary>
    /// Removes the user's configuration of one two-factor method.
    /// </summary>
    /// <param name="id">The user.</param>
    /// <param name="method">The method name, see <see cref="TwoFactorMethod.Method"/>.</param>
    Task RemoveUserTwoFactorMethodAsync(UserId id, string method);

    /// <summary>
    /// Removes the user's configuration of all two-factor methods, e.g. after they lost their device.
    /// </summary>
    Task RemoveAllUserTwoFactorMethodsAsync(UserId id);

    /// <summary>
    /// Gets the history of a user, e.g. attribute changes and added or removed organizations.
    /// </summary>
    /// <remarks>
    /// Needs the <c>ticket.agent</c> or <c>admin.user</c> permission. Returns <c>null</c> if the user doesn't exist.
    /// </remarks>
    Task<HistoryList?> GetUserHistoryAsync(UserId id);
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

    public async Task DeleteUserAsync(UserId id) => await DeleteAsync<bool>($"{UsersEndpoint}/{id}");

    public async Task UnlockUserAsync(UserId id) => await PutAsync<object>($"{UsersEndpoint}/unlock/{id}");

    public async Task<List<TwoFactorMethod>> ListUserTwoFactorMethodsAsync(UserId id) =>
        await GetAsync<List<TwoFactorMethod>>($"{UsersEndpoint}/{id}/admin_two_factor/enabled_authentication_methods")
        ?? [];

    public async Task RemoveUserTwoFactorMethodAsync(UserId id, string method) =>
        await DeleteAsync<object>(
            $"{UsersEndpoint}/{id}/admin_two_factor/remove_authentication_method",
            new TwoFactorMethodRequest { Method = method }
        );

    public async Task RemoveAllUserTwoFactorMethodsAsync(UserId id) =>
        await DeleteAsync<object>($"{UsersEndpoint}/{id}/admin_two_factor/remove_all_authentication_methods");

    public async Task<HistoryList?> GetUserHistoryAsync(UserId id) =>
        await GetAsync<HistoryList>($"{UsersEndpoint}/history/{id}");
}
