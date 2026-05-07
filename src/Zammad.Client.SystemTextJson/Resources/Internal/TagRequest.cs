using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class TagRequest
{
    [JsonPropertyName("object")]
    public required string ObjectName { get; set; }

    [JsonPropertyName("o_id")]
    public required ObjectId ObjectId { get; set; }

    [JsonPropertyName("item")]
    public required string Item { get; set; }
}
