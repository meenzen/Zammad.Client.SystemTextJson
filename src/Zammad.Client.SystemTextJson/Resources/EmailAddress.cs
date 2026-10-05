using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class EmailAddress
{
    [JsonPropertyName("id")]
    public EmailAddressId Id { get; set; }

    [JsonPropertyName("channel_id")]
    public ChannelId? ChannelId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Whether the email address is active.
    /// </summary>
    /// <remarks>
    /// Zammad manages this itself: an email address is only active if it is assigned to an existing channel.
    /// </remarks>
    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("preferences")]
    public Dictionary<string, JsonElement>? Preferences { get; set; }

    [JsonPropertyName("group_ids")]
    public List<GroupId>? GroupIds { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
