using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class LinkAddRequest
{
    [JsonPropertyName("link_type")]
    public required LinkType LinkType { get; set; }

    [JsonPropertyName("link_object_target")]
    public required string TargetObject { get; set; }

    [JsonPropertyName("link_object_target_value")]
    public required TargetObjectId TargetValue { get; set; }

    [JsonPropertyName("link_object_source")]
    public required string SourceObject { get; set; }

    /// <summary>
    /// The ticket number for tickets, the ID for knowledge base answer translations.
    /// </summary>
    [JsonPropertyName("link_object_source_number")]
    public required string SourceNumber { get; set; }
}

internal sealed class LinkRemoveRequest
{
    [JsonPropertyName("link_type")]
    public required LinkType LinkType { get; set; }

    [JsonPropertyName("link_object_target")]
    public required string TargetObject { get; set; }

    [JsonPropertyName("link_object_target_value")]
    public required TargetObjectId TargetValue { get; set; }

    [JsonPropertyName("link_object_source")]
    public required string SourceObject { get; set; }

    [JsonPropertyName("link_object_source_value")]
    public required TargetObjectId SourceValue { get; set; }
}
