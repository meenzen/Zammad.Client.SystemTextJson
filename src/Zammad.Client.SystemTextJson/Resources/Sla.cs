using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// A service level agreement. All times are in minutes of business hours of <see cref="CalendarId"/>.
/// </summary>
public sealed class Sla
{
    [JsonPropertyName("id")]
    public SlaId Id { get; set; }

    [JsonPropertyName("calendar_id")]
    public CalendarId? CalendarId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Minutes until the first response.
    /// </summary>
    [JsonPropertyName("first_response_time")]
    public int? FirstResponseTime { get; set; }

    /// <summary>
    /// Minutes between a customer message and the next agent response. Can't be combined with
    /// <see cref="UpdateTime"/>.
    /// </summary>
    [JsonPropertyName("response_time")]
    public int? ResponseTime { get; set; }

    /// <summary>
    /// Minutes between two agent updates. Can't be combined with <see cref="ResponseTime"/>.
    /// </summary>
    [JsonPropertyName("update_time")]
    public int? UpdateTime { get; set; }

    /// <summary>
    /// Minutes until the ticket is closed.
    /// </summary>
    [JsonPropertyName("solution_time")]
    public int? SolutionTime { get; set; }

    /// <summary>
    /// Ticket selector, e.g. <c>{"ticket.priority_id": {"operator": "is", "value": ["3"]}}</c>.
    /// </summary>
    /// <remarks>
    /// Build values with <see cref="JsonSerializer.SerializeToElement{TValue}(TValue, JsonSerializerOptions?)"/>.
    /// If several SLAs match a ticket, Zammad uses the first by name.
    /// </remarks>
    [JsonPropertyName("condition")]
    public JsonElement? Condition { get; set; }

    /// <summary>
    /// Unused by Zammad; always empty.
    /// </summary>
    [JsonPropertyName("data")]
    public Dictionary<string, JsonElement>? Data { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
