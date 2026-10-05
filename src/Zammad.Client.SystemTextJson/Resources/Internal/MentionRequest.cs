using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class MentionRequest
{
    [JsonPropertyName("mentionable_type")]
    public required string MentionableType { get; set; }

    [JsonPropertyName("mentionable_id")]
    public required TargetObjectId MentionableId { get; set; }
}

internal sealed class MentionList
{
    [JsonPropertyName("mentions")]
    public List<Mention>? Mentions { get; set; }
}
