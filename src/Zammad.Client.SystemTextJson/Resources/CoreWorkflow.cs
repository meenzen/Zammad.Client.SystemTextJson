using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class CoreWorkflow
{
    [JsonPropertyName("id")]
    public CoreWorkflowId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The object whose forms the workflow changes: <c>Ticket</c>, <c>User</c>, <c>Organization</c> or
    /// <c>Group</c>. Empty for all objects.
    /// </summary>
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    /// <summary>
    /// Settings, mainly <c>{"screen": ["create_middle", "edit"]}</c>: the form screens the workflow applies to.
    /// </summary>
    [JsonPropertyName("preferences")]
    public Dictionary<string, JsonElement>? Preferences { get; set; }

    /// <summary>
    /// Selector matched against the saved object, e.g.
    /// <c>{"ticket.state_id": {"operator": "is", "value": ["1"]}}</c>.
    /// </summary>
    /// <remarks>
    /// Build values with <see cref="JsonSerializer.SerializeToElement{TValue}(TValue, JsonSerializerOptions?)"/>.
    /// </remarks>
    [JsonPropertyName("condition_saved")]
    public JsonElement? ConditionSaved { get; set; }

    /// <summary>
    /// Selector matched against the values currently selected in the form.
    /// </summary>
    [JsonPropertyName("condition_selected")]
    public JsonElement? ConditionSelected { get; set; }

    /// <summary>
    /// Field changes, e.g. <c>{"ticket.priority_id": {"operator": "remove_option", "remove_option": ["3"]}}</c>.
    /// </summary>
    [JsonPropertyName("perform")]
    public JsonElement? Perform { get; set; }

    /// <summary>
    /// Whether the workflow applies. Active by default.
    /// </summary>
    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    /// <summary>
    /// Whether to skip all workflows with a higher <see cref="Priority"/> after this one matched.
    /// </summary>
    [JsonPropertyName("stop_after_match")]
    public bool? StopAfterMatch { get; set; }

    /// <summary>
    /// <c>false</c> for Zammad's built-in workflows, which the API hides.
    /// </summary>
    [JsonPropertyName("changeable")]
    public bool? Changeable { get; set; }

    /// <summary>
    /// Workflows run in ascending order of priority.
    /// </summary>
    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
