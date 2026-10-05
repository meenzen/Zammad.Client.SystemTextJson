using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// A webhook type with a payload prepared by Zammad, e.g. for Slack or Mattermost.
/// </summary>
public sealed class PreDefinedWebhook
{
    /// <summary>
    /// The value for <see cref="Webhook.PreDefinedWebhookType"/>, e.g. <c>Slack</c>.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The payload template Zammad sends (a JSON document as a string).
    /// </summary>
    [JsonPropertyName("custom_payload")]
    public string? CustomPayload { get; set; }

    /// <summary>
    /// Additional settings, stored in <see cref="Webhook.Preferences"/> under <c>pre_defined_webhook</c>.
    /// </summary>
    [JsonPropertyName("fields")]
    public List<PreDefinedWebhookField>? Fields { get; set; }

    [JsonPropertyName("field_names")]
    public List<string>? FieldNames { get; set; }
}

public sealed class PreDefinedWebhookField
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("display")]
    public string? Display { get; set; }

    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// The default value, if any.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("null")]
    public bool? Null { get; set; }

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }
}
