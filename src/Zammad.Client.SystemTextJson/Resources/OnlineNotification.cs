using System;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public class OnlineNotification
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("o_id")]
    public int? ObjectId { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("seen")]
    public bool? Seen { get; set; }

    [JsonPropertyName("updated_by_id")]
    public int? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public int? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
