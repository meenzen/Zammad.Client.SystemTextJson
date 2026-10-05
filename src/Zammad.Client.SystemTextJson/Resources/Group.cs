using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class Group : IHasCustomFields
{
    [JsonPropertyName("id")]
    public GroupId Id { get; set; }

    [JsonPropertyName("signature_id")]
    public SignatureId? SignatureId { get; set; }

    [JsonPropertyName("email_address_id")]
    public EmailAddressId? EmailAddressId { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Minutes after which a ticket is unassigned if its owner doesn't work on it.
    /// </summary>
    [JsonPropertyName("assignment_timeout")]
    public int? AssignmentTimeout { get; set; }

    [JsonPropertyName("follow_up_possible")]
    public string? FollowUpPossible { get; set; }

    [JsonPropertyName("follow_up_assignment")]
    public bool? FollowUpAssignment { get; set; }

    [JsonPropertyName("active")]
    public required bool Active { get; set; }

    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;

    [JsonPropertyName("user_ids")]
    public List<UserId>? UserIds { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Last part of the name of a nested group, the name contains the full path.
    /// </summary>
    [JsonPropertyName("name_last")]
    public string? NameLast { get; set; }

    [JsonPropertyName("parent_id")]
    public GroupId? ParentId { get; set; }

    [JsonPropertyName("reopen_time_in_days")]
    public int? ReopenTimeInDays { get; set; }

    [JsonPropertyName("shared_drafts")]
    public bool? SharedDrafts { get; set; }

    [JsonPropertyName("summary_generation")]
    public string? SummaryGeneration { get; set; }

    [JsonPropertyName("email_address")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    [JsonPropertyName("users")]
    public List<string>? Users { get; set; }

    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("updated_by")]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Additional properties that are not explicitly defined in this class.
    /// </summary>
    /// <remarks>
    /// This will contain custom attributes configured in the Zammad object manager.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
