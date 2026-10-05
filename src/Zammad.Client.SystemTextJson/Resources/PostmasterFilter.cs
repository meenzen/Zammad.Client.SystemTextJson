using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class PostmasterFilter
{
    [JsonPropertyName("id")]
    public PostmasterFilterId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The channel type the filter applies to. Required; Zammad only uses <c>email</c>.
    /// </summary>
    [JsonPropertyName("channel")]
    public string? Channel { get; set; }

    /// <summary>
    /// Rules by email header, e.g. <c>{"from": {"operator": "contains", "value": "@example.com"}}</c>. All rules
    /// must match.
    /// </summary>
    /// <remarks>
    /// Build values with <see cref="JsonSerializer.SerializeToElement{TValue}(TValue, JsonSerializerOptions?)"/>.
    /// Operators: <c>contains</c>, <c>contains not</c>, <c>is any of</c>, <c>is none of</c>,
    /// <c>starts with one of</c>, <c>ends with one of</c>, <c>matches regex</c>, <c>does not match regex</c>.
    /// </remarks>
    [JsonPropertyName("match")]
    public JsonElement? Match { get; set; }

    /// <summary>
    /// <c>X-Zammad-*</c> headers to set, e.g. <c>{"x-zammad-ticket-priority_id": {"value": "3"}}</c>.
    /// </summary>
    [JsonPropertyName("perform")]
    public JsonElement? Perform { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
