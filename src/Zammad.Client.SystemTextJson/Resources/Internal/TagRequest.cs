using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class TagRequest
{
    [JsonPropertyName("object")]
    public required ObjectType ObjectType { get; set; }

    [JsonPropertyName("o_id")]
    public required TargetObjectId ObjectId { get; set; }

    [JsonPropertyName("item")]
    public required string Item { get; set; }
}
