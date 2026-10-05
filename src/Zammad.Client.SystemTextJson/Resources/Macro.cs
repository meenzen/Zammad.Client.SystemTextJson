using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class Macro
{
    [JsonPropertyName("id")]
    public MacroId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The changes the macro applies, keyed by attribute, e.g.
    /// <c>{"ticket.state_id": {"value": "4"}, "ticket.tags": {"operator": "add", "value": "spam"}}</c>.
    /// </summary>
    /// <remarks>
    /// Build a value with <see cref="JsonSerializer.SerializeToElement{TValue}(TValue, JsonSerializerOptions?)"/>.
    /// </remarks>
    [JsonPropertyName("perform")]
    public JsonElement? Perform { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    /// <summary>
    /// What the UI does after the macro was applied: <c>none</c> (default), <c>next_task</c>,
    /// <c>next_task_on_close</c> or <c>next_from_overview</c>.
    /// </summary>
    [JsonPropertyName("ux_flow_next_up")]
    public string? UxFlowNextUp { get; set; }

    /// <summary>
    /// At most 250 characters.
    /// </summary>
    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// The groups the macro is available in. Empty means all groups.
    /// </summary>
    [JsonPropertyName("group_ids")]
    public List<GroupId>? GroupIds { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Group names, only set in expanded responses.
    /// </summary>
    [JsonPropertyName("groups")]
    public List<string>? Groups { get; set; }

    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("updated_by")]
    public string? UpdatedBy { get; set; }
}
