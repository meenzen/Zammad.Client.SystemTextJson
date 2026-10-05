using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class TextModule
{
    [JsonPropertyName("id")]
    public TextModuleId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Keywords that trigger the text module in the editor (after <c>::</c>), separated by spaces or commas.
    /// </summary>
    [JsonPropertyName("keywords")]
    public string? Keywords { get; set; }

    /// <summary>
    /// The text, as HTML. It may contain placeholders such as <c>#{ticket.customer.firstname}</c>.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// At most 250 characters.
    /// </summary>
    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    /// <summary>
    /// The groups the text module is available in. Empty means all groups.
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
