using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class UserAccessTokenRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("permission")]
    public required List<string> Permission { get; set; }

    /// <summary>
    /// A date (<c>yyyy-MM-dd</c>). Zammad ignores the time of day.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public string? ExpiresAt { get; set; }
}

internal sealed class UserAccessTokenResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}
