using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class DataPrivacyTaskRequest
{
    [JsonPropertyName("deletable_type")]
    public required string DeletableType { get; init; }

    [JsonPropertyName("deletable_id")]
    public required TargetObjectId DeletableId { get; init; }

    [JsonPropertyName("preferences")]
    public Dictionary<string, string>? Preferences { get; init; }
}
