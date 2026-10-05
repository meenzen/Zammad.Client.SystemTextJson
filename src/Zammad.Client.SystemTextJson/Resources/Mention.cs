using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// A user's subscription to a ticket. Subscribed users get notified about changes to the ticket.
/// </summary>
public sealed class Mention
{
    [JsonPropertyName("id")]
    public MentionId Id { get; set; }

    /// <summary>
    /// Model name of the subscribed object. Zammad only supports <c>Ticket</c>.
    /// </summary>
    [JsonPropertyName("mentionable_type")]
    public string? MentionableType { get; set; }

    [JsonPropertyName("mentionable_id")]
    public TargetObjectId? MentionableId { get; set; }

    /// <summary>
    /// The subscribed user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public UserId? UserId { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
