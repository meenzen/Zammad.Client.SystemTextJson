using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class OnlineNotification
{
    [JsonPropertyName("id")]
    public NotificationId Id { get; set; }

    [JsonPropertyName("o_id")]
    public TargetObjectId? ObjectId { get; set; }

    [JsonPropertyName("object")]
    public ObjectType? ObjectType { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("seen")]
    public bool? Seen { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
