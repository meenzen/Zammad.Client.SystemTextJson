using System.Globalization;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

/// <summary>
/// Personal API tokens of the current user (the user named in <see cref="ZammadOptions.OnBehalfOf"/>, if set).
/// </summary>
/// <remarks>
/// Requires the <c>user_preferences.access_token</c> permission. A token authenticates as the user who created it,
/// limited to the permissions it was created with (see <see cref="ZammadOptions.Token"/>).
/// </remarks>
public interface IUserAccessTokenService
{
    /// <summary>
    /// Lists the current user's tokens and the permissions they can grant to a new token.
    /// </summary>
    Task<UserAccessTokenList> ListUserAccessTokensAsync();

    /// <summary>
    /// Creates a token for the current user and returns its value.
    /// </summary>
    /// <remarks>
    /// The value is only returned this once, and the response has no ID: use
    /// <see cref="ListUserAccessTokensAsync"/> to find the new token's ID. Zammad rejects this with 422 if the
    /// <c>api_token_access</c> setting is disabled.
    /// </remarks>
    /// <param name="name">A name to tell the token apart from the others. Doesn't have to be unique.</param>
    /// <param name="permissions">
    /// The permissions to grant to the token, e.g. <c>ticket.agent</c>, see <see cref="UserAccessTokenList.Permissions"/>.
    /// </param>
    /// <param name="expiresAt">
    /// The day on which the token expires. Zammad ignores the time of day and lets the token expire at the start of
    /// that day in its default time zone. <c>null</c> for a token that never expires.
    /// </param>
    /// <returns>The token, to be used as <see cref="ZammadOptions.Token"/>.</returns>
    Task<string> CreateUserAccessTokenAsync(string name, IEnumerable<string> permissions, DateTime? expiresAt = null);

    /// <summary>
    /// Deletes one of the current user's tokens.
    /// </summary>
    /// <remarks>
    /// Admins can't delete other users' tokens this way either. Zammad responds with 422 (not 404) if the current user
    /// has no token with this ID.
    /// </remarks>
    Task DeleteUserAccessTokenAsync(UserAccessTokenId id);
}

public sealed partial class ZammadClient : IUserAccessTokenService
{
    private const string UserAccessTokenEndpoint = "/api/v1/user_access_token";

    public async Task<UserAccessTokenList> ListUserAccessTokensAsync() =>
        await GetAsync<UserAccessTokenList>(UserAccessTokenEndpoint) ?? throw LogicException.UnexpectedNullResult;

    public async Task<string> CreateUserAccessTokenAsync(
        string name,
        IEnumerable<string> permissions,
        DateTime? expiresAt = null
    )
    {
        var request = new UserAccessTokenRequest
        {
            Name = name,
            Permission = permissions.ToList(),
            ExpiresAt = expiresAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        };
        var response = await PostAsync<UserAccessTokenResponse>(UserAccessTokenEndpoint, request);
        return response?.Token ?? throw LogicException.UnexpectedNullResult;
    }

    public async Task DeleteUserAccessTokenAsync(UserAccessTokenId id) =>
        await DeleteAsync<object>($"{UserAccessTokenEndpoint}/{id}");
}
