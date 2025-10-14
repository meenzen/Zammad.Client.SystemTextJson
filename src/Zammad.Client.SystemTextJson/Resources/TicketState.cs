using System;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public class TicketState
{
    [JsonPropertyName("id")]
    public StateId Id { get; set; }

    [JsonPropertyName("state_type_id")]
    public StateTypeId? StateTypeId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("next_state_id")]
    public StateId? NextStateId { get; set; }

    [JsonPropertyName("ignore_escalation")]
    public bool? IgnoreEscalation { get; set; }

    [JsonPropertyName("default_create")]
    public bool? DefaultCreate { get; set; }

    [JsonPropertyName("default_follow_up")]
    public bool? DefaultFollowUp { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
