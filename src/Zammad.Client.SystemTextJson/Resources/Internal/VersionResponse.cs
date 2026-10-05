using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class VersionResponse
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
