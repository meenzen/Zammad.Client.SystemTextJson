using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class Trigger
{
    [JsonPropertyName("id")]
    public TriggerId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Ticket selector, e.g. <c>{"ticket.state_id": {"operator": "is", "value": ["1"]}}</c>.
    /// </summary>
    /// <remarks>
    /// Build values with <see cref="JsonSerializer.SerializeToElement{TValue}(TValue, JsonSerializerOptions?)"/>.
    /// For action-based triggers, <c>ticket.action</c> (<c>create</c>/<c>update</c>) restricts the trigger to
    /// ticket creation or updates.
    /// </remarks>
    [JsonPropertyName("condition")]
    public JsonElement? Condition { get; set; }

    /// <summary>
    /// Changes to perform, e.g. <c>{"ticket.priority_id": {"value": "3"}}</c> or
    /// <c>{"notification.webhook": {"webhook_id": "1"}}</c>.
    /// </summary>
    /// <remarks>
    /// Build values with <see cref="JsonSerializer.SerializeToElement{TValue}(TValue, JsonSerializerOptions?)"/>.
    /// </remarks>
    [JsonPropertyName("perform")]
    public JsonElement? Perform { get; set; }

    [JsonPropertyName("disable_notification")]
    public bool? DisableNotification { get; set; }

    [JsonPropertyName("localization")]
    public string? Localization { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// <c>action</c> (default, runs when a ticket is created or updated) or <c>time</c> (runs when a ticket reaches
    /// a time event such as escalation or a pending reminder).
    /// </summary>
    [JsonPropertyName("activator")]
    public string? Activator { get; set; }

    /// <summary>
    /// For action-based triggers: <c>selective</c> (default, runs only if an attribute in the condition changed)
    /// or <c>always</c>.
    /// </summary>
    [JsonPropertyName("execution_condition_mode")]
    public string? ExecutionConditionMode { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
