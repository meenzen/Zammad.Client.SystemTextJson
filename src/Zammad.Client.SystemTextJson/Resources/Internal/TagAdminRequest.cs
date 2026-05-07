using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class TagAdminRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
