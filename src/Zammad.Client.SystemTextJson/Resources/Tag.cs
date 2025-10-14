using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public class Tag
{
    [JsonPropertyName("id")]
    public TagId Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
