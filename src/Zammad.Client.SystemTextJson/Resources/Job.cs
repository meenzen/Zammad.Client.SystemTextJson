using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// A scheduler, which performs changes on all objects that match <see cref="Condition"/> at the times in
/// <see cref="Timeplan"/>. Zammad's API and model call it a job.
/// </summary>
public sealed class Job
{
    [JsonPropertyName("id")]
    public JobId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// When the scheduler runs, e.g.
    /// <c>{"days": {"Mon": true}, "hours": {"9": true}, "minutes": {"0": true}}</c>. Minutes are in steps of 10.
    /// </summary>
    /// <remarks>
    /// Build values with <see cref="JsonSerializer.SerializeToElement{TValue}(TValue, JsonSerializerOptions?)"/>.
    /// </remarks>
    [JsonPropertyName("timeplan")]
    public JsonElement? Timeplan { get; set; }

    /// <summary>
    /// <c>Ticket</c>, <c>User</c> or <c>Organization</c>.
    /// </summary>
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    /// <summary>
    /// Selector for the objects to change, e.g. <c>{"ticket.state_id": {"operator": "is", "value": ["1"]}}</c>.
    /// </summary>
    [JsonPropertyName("condition")]
    public JsonElement? Condition { get; set; }

    /// <summary>
    /// Changes to perform on every matching object, e.g. <c>{"ticket.priority_id": {"value": "3"}}</c>.
    /// </summary>
    [JsonPropertyName("perform")]
    public JsonElement? Perform { get; set; }

    [JsonPropertyName("disable_notification")]
    public bool? DisableNotification { get; set; }

    [JsonPropertyName("last_run_at")]
    public DateTimeOffset? LastRunAt { get; set; }

    /// <summary>
    /// When the scheduler runs next. Only set for active schedulers.
    /// </summary>
    [JsonPropertyName("next_run_at")]
    public DateTimeOffset? NextRunAt { get; set; }

    [JsonPropertyName("running")]
    public bool? Running { get; set; }

    /// <summary>
    /// Number of objects processed in the last run.
    /// </summary>
    [JsonPropertyName("processed")]
    public int? Processed { get; set; }

    /// <summary>
    /// Number of objects matching <see cref="Condition"/>, calculated by Zammad when the scheduler is saved or run.
    /// </summary>
    [JsonPropertyName("matching")]
    public int? Matching { get; set; }

    [JsonPropertyName("pid")]
    public string? Pid { get; set; }

    [JsonPropertyName("localization")]
    public string? Localization { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// Whether the scheduler runs. Inactive by default.
    /// </summary>
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
