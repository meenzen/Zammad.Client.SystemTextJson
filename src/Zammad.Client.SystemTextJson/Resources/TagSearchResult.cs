using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class TagSearchResult
{
    [JsonPropertyName("id")]
    public required TagId Id { get; set; }

    [JsonPropertyName("value")]
    public required string Value { get; set; }
}
