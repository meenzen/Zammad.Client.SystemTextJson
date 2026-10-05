using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// The history (audit log) of a ticket, user or organization.
/// </summary>
public sealed class HistoryList
{
    /// <summary>
    /// The entries, oldest first.
    /// </summary>
    [JsonPropertyName("history")]
    public List<HistoryEntry> Entries { get; set; } = [];

    /// <summary>
    /// The records the entries refer to and the users who made the changes.
    /// </summary>
    [JsonPropertyName("assets")]
    public Assets Assets { get; set; } = new();
}

/// <summary>
/// One change in a <see cref="HistoryList"/>.
/// </summary>
/// <remarks>
/// Zammad leaves out <see cref="IdFrom"/>/<see cref="IdTo"/>, <see cref="ValueFrom"/>/<see cref="ValueTo"/> and
/// <see cref="RelatedObjectId"/> when they are empty.
/// </remarks>
public sealed class HistoryEntry
{
    [JsonPropertyName("id")]
    public HistoryId Id { get; set; }

    /// <summary>
    /// What happened, e.g. <c>created</c>, <c>updated</c>, <c>removed</c>, <c>added</c>, <c>merged_into</c>,
    /// <c>received_merge</c>, <c>email</c> or <c>notification</c>.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Model name of the changed record, e.g. <c>Ticket</c>, <c>Ticket::Article</c>, <c>Mention</c>,
    /// <c>Checklist</c>, <c>User</c> or <c>Organization</c>.
    /// </summary>
    /// <remarks>
    /// A ticket's history also contains the changes of its articles, mentions, shared drafts and checklists. Their
    /// <see cref="RelatedObject"/> is <c>Ticket</c>.
    /// </remarks>
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    /// <summary>
    /// ID of the changed record, of the model in <see cref="Object"/>.
    /// </summary>
    [JsonPropertyName("o_id")]
    public TargetObjectId ObjectId { get; set; }

    /// <summary>
    /// The changed attribute without the <c>_id</c> suffix, e.g. <c>state</c> for <c>state_id</c>, or <c>null</c>
    /// if the entry isn't about one attribute.
    /// </summary>
    [JsonPropertyName("attribute")]
    public string? Attribute { get; set; }

    /// <summary>
    /// Always <c>null</c>. Zammad only replaces it with <see cref="Attribute"/> when it is set.
    /// </summary>
    [JsonPropertyName("history_attribute_id")]
    public int? HistoryAttributeId { get; set; }

    /// <summary>
    /// The old value. For <c>_id</c> attributes the name of the referenced record (e.g. the state name), with the ID
    /// in <see cref="IdFrom"/>.
    /// </summary>
    [JsonPropertyName("value_from")]
    public string? ValueFrom { get; set; }

    [JsonPropertyName("value_to")]
    public string? ValueTo { get; set; }

    /// <summary>
    /// The old ID for <c>_id</c> attributes. For merges, the ID of the merged (source) ticket.
    /// </summary>
    [JsonPropertyName("id_from")]
    public TargetObjectId? IdFrom { get; set; }

    /// <summary>
    /// The new ID for <c>_id</c> attributes. For merges, the ID of the target ticket.
    /// </summary>
    [JsonPropertyName("id_to")]
    public TargetObjectId? IdTo { get; set; }

    /// <summary>
    /// Model name of the record this entry belongs to, e.g. <c>Ticket</c> for article changes.
    /// </summary>
    [JsonPropertyName("related_object")]
    public string? RelatedObject { get; set; }

    /// <summary>
    /// Always <c>null</c>. Zammad only replaces it with <see cref="RelatedObject"/> when it is set.
    /// </summary>
    [JsonPropertyName("related_history_object_id")]
    public int? RelatedHistoryObjectId { get; set; }

    /// <summary>
    /// ID of the record in <see cref="RelatedObject"/>.
    /// </summary>
    [JsonPropertyName("related_o_id")]
    public TargetObjectId? RelatedObjectId { get; set; }

    /// <summary>
    /// Model name of what made the change, if it was automated: <c>Trigger</c>, <c>Job</c> (scheduler),
    /// <c>PostmasterFilter</c> or <c>AI::Agent</c>.
    /// </summary>
    [JsonPropertyName("sourceable_type")]
    public string? SourceableType { get; set; }

    [JsonPropertyName("sourceable_id")]
    public TargetObjectId? SourceableId { get; set; }

    /// <summary>
    /// Name of the trigger, scheduler, ... at the time of the change.
    /// </summary>
    [JsonPropertyName("sourceable_name")]
    public string? SourceableName { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }
}
