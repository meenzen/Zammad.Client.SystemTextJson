using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

#nullable enable
public class Tag
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
