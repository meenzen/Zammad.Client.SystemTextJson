using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// An activity type that time accounting entries can be assigned to (<see cref="TicketAccounting.TypeId"/>).
/// </summary>
public sealed class TimeAccountingType
{
    [JsonPropertyName("id")]
    public TimeAccountingTypeId Id { get; set; }

    /// <summary>
    /// The name, unique (case-insensitive).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// Whether the type is active. Types can't be deleted, deactivate them instead.
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
