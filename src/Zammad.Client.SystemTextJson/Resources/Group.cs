using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public class Group
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("signature_id")]
    public int? SignatureId { get; set; }

    [JsonPropertyName("email_address_id")]
    public int? EmailAddressId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("assignment_timeout")]
    public TimeSpan? AssignmentTimeout { get; set; }

    [JsonPropertyName("follow_up_possible")]
    public string FollowUpPossible { get; set; }

    [JsonPropertyName("follow_up_assignment")]
    public bool FollowUpAssignment { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("note")]
    public string Note { get; set; }

    [JsonPropertyName("user_ids")]
    public List<int> UserIds { get; set; }

    [JsonPropertyName("updated_by_id")]
    public int UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public int CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}
