using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public class Organization
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("shared")]
    public required bool Shared { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("domain_assignment")]
    public bool DomainAssignment { get; set; }

    [JsonPropertyName("active")]
    public required bool Active { get; set; }

    [JsonPropertyName("vip")]
    public bool Vip { get; set; }

    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;

    [JsonPropertyName("member_ids")]
    public List<int>? MemberIds { get; set; }

    [JsonPropertyName("updated_by_id")]
    public int? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public int? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
