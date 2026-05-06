using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class TagAdminRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
