using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// How a linked object relates to the object whose links were listed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LinkType
{
    [JsonStringEnumMemberName("normal")]
    Normal,

    /// <summary>
    /// The linked object is the parent of the object whose links were listed.
    /// </summary>
    [JsonStringEnumMemberName("parent")]
    Parent,

    /// <summary>
    /// The linked object is a child of the object whose links were listed.
    /// </summary>
    [JsonStringEnumMemberName("child")]
    Child,
}

/// <summary>
/// A link as seen from the object whose links were listed.
/// </summary>
public sealed class Link
{
    /// <summary>
    /// What the linked object is to the object whose links were listed.
    /// </summary>
    [JsonPropertyName("link_type")]
    public LinkType? LinkType { get; set; }

    /// <summary>
    /// Model name of the linked object, <c>Ticket</c> or <c>KnowledgeBase::Answer::Translation</c>.
    /// </summary>
    [JsonPropertyName("link_object")]
    public string? LinkObject { get; set; }

    /// <summary>
    /// ID of the linked object.
    /// </summary>
    [JsonPropertyName("link_object_value")]
    public TargetObjectId? LinkObjectValue { get; set; }

    /// <summary>
    /// ID of the linked ticket, or <c>null</c> if the linked object isn't a ticket.
    /// </summary>
    [JsonIgnore]
    public TicketId? TicketId =>
        LinkObject == "Ticket" && LinkObjectValue is { } value ? new TicketId(value.Value) : null;
}

public sealed class LinkList
{
    [JsonPropertyName("links")]
    public List<Link> Links { get; set; } = [];

    /// <summary>
    /// The linked objects and the records they reference (users, organizations, ...).
    /// </summary>
    [JsonPropertyName("assets")]
    public Assets Assets { get; set; } = new();
}
