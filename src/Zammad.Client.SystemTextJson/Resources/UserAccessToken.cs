using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// A personal API token. The token value itself is only returned once, when the token is created.
/// </summary>
public sealed class UserAccessToken
{
    [JsonPropertyName("id")]
    public UserAccessTokenId Id { get; set; }

    [JsonPropertyName("user_id")]
    public UserId? UserId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Always <c>api</c> for user access tokens.
    /// </summary>
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("preferences")]
    public UserAccessTokenPreferences? Preferences { get; set; }

    [JsonPropertyName("last_used_at")]
    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    /// The start of the day on which the token expires, in Zammad's default time zone.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class UserAccessTokenPreferences
{
    /// <summary>
    /// The permissions the token is limited to, e.g. <c>ticket.agent</c>.
    /// </summary>
    [JsonPropertyName("permission")]
    public List<string>? Permission { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public sealed class UserAccessTokenList
{
    [JsonPropertyName("tokens")]
    public List<UserAccessToken> Tokens { get; set; } = [];

    /// <summary>
    /// The permissions the current user can grant to a token: their own permissions, the child permissions and the
    /// parent permissions. Parents the user doesn't have themselves have <c>preferences.disabled</c> set.
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<Permission> Permissions { get; set; } = [];
}
