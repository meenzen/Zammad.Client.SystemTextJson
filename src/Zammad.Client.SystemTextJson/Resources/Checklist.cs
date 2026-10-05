using System.Text.Json.Serialization;
using Zammad.Client.Core;

namespace Zammad.Client.Resources;

/// <summary>
/// The checklist of a ticket. A ticket has at most one checklist, see <see cref="Ticket.ChecklistId"/>.
/// </summary>
public sealed class Checklist
{
    [JsonPropertyName("id")]
    public ChecklistId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The order of the items. Set it in <see cref="IChecklistService.UpdateChecklistAsync"/> to reorder the items.
    /// </summary>
    [JsonPropertyName("sorted_item_ids")]
    [JsonConverter(typeof(ChecklistItemIdListConverter))]
    public List<ChecklistItemId>? SortedItemIds { get; set; }

    /// <summary>
    /// The IDs of the items in no particular order.
    /// </summary>
    [JsonPropertyName("item_ids")]
    public List<ChecklistItemId>? ItemIds { get; set; }

    /// <summary>
    /// Only set in <see cref="Assets"/>, if the current user can't access the checklist's ticket.
    /// </summary>
    [JsonPropertyName("ticket_inaccessible")]
    public bool? TicketInaccessible { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// The items in the order of <see cref="SortedItemIds"/>.
    /// </summary>
    /// <remarks>
    /// Zammad doesn't include the items in the checklist record. <see cref="IChecklistService"/> fills this list from
    /// the assets of the response. It is <c>null</c> in <see cref="Assets"/>.
    /// </remarks>
    [JsonIgnore]
    public List<ChecklistItem>? Items { get; set; }
}

public sealed class ChecklistItem
{
    [JsonPropertyName("id")]
    public ChecklistItemId Id { get; set; }

    [JsonPropertyName("checklist_id")]
    public ChecklistId? ChecklistId { get; set; }

    /// <summary>
    /// The text of the item. If it contains a ticket number with the ticket hook (e.g. <c>Ticket#31001</c>), Zammad
    /// links the item to that ticket, see <see cref="TicketId"/>.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Whether the item is done. For items that reference a ticket, Zammad sets this from the ticket's state (closed or
    /// merged) and updates it when the state changes.
    /// </summary>
    [JsonPropertyName("checked")]
    public bool? Checked { get; set; }

    /// <summary>
    /// The ticket that the item references, not the ticket the checklist belongs to.
    /// </summary>
    [JsonPropertyName("ticket_id")]
    public TicketId? TicketId { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class ChecklistTemplate
{
    [JsonPropertyName("id")]
    public ChecklistTemplateId Id { get; set; }

    /// <summary>
    /// The name of the template. Checklists created from the template get the same name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Only active templates can be used to create checklists.
    /// </summary>
    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    /// <summary>
    /// The texts of the items in order.
    /// </summary>
    /// <remarks>
    /// Zammad doesn't include the items in the template record. <see cref="IChecklistService"/> fills this list from
    /// the assets of the response. It is <c>null</c> in <see cref="Assets"/>. When updating a template, <c>null</c> or
    /// an empty list keeps the items, any other value replaces all of them. Zammad trims the texts and drops blank
    /// ones. A template can have at most 100 items.
    /// </remarks>
    [JsonIgnore]
    public List<string>? Items { get; set; }

    [JsonPropertyName("sorted_item_ids")]
    [JsonConverter(typeof(ChecklistTemplateItemIdListConverter))]
    public List<ChecklistTemplateItemId>? SortedItemIds { get; set; }

    [JsonPropertyName("item_ids")]
    public List<ChecklistTemplateItemId>? ItemIds { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class ChecklistTemplateItem
{
    [JsonPropertyName("id")]
    public ChecklistTemplateItemId Id { get; set; }

    [JsonPropertyName("checklist_template_id")]
    public ChecklistTemplateId? ChecklistTemplateId { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
