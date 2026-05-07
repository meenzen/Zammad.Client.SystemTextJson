using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class HealthCheckResult
{
    [JsonPropertyName("healthy")]
    public required bool Healthy { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("issues")]
    public required List<JsonElement> Issues { get; set; }

    [JsonPropertyName("actions")]
    public required List<JsonElement> Actions { get; set; }

    [JsonPropertyName("token")]
    public required string Token { get; set; }
}
