using System.Text.Json.Serialization;
using Zammad.Client.Core;

namespace Zammad.Client.Resources.Internal;

internal sealed class ChecklistCreateRequest
{
    [JsonPropertyName("ticket_id")]
    public required TicketId TicketId { get; set; }

    [JsonPropertyName("template_id")]
    public ChecklistTemplateId? TemplateId { get; set; }

    [JsonPropertyName("create_first_item")]
    public bool? CreateFirstItem { get; set; }
}

internal sealed class ChecklistUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("sorted_item_ids")]
    [JsonConverter(typeof(ChecklistItemIdListConverter))]
    public List<ChecklistItemId>? SortedItemIds { get; set; }
}

internal sealed class ChecklistItemRequest
{
    [JsonPropertyName("checklist_id")]
    public ChecklistId? ChecklistId { get; set; }

    /// <summary>
    /// The ticket whose checklist the item is added to. Zammad creates the checklist if the ticket has none.
    /// </summary>
    [JsonPropertyName("ticket_id")]
    public TicketId? TicketId { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("checked")]
    public bool? Checked { get; set; }
}

internal sealed class ChecklistItemBulkRequest
{
    [JsonPropertyName("checklist_id")]
    public ChecklistId? ChecklistId { get; set; }

    [JsonPropertyName("ticket_id")]
    public TicketId? TicketId { get; set; }

    [JsonPropertyName("items")]
    public required List<ChecklistItemRequest> Items { get; set; }
}

internal sealed class ChecklistItemBulkResponse
{
    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("checklist_item_ids")]
    public List<ChecklistItemId>? ChecklistItemIds { get; set; }
}

internal sealed class ChecklistTemplateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("items")]
    public List<string>? Items { get; set; }
}

/// <summary>
/// The response of endpoints called with <c>full=true</c>, and of <c>POST /checklists</c>.
/// </summary>
internal sealed class FullResponse
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("record_ids")]
    public List<int>? RecordIds { get; set; }

    [JsonPropertyName("total_count")]
    public int? TotalCount { get; set; }

    [JsonPropertyName("assets")]
    public Assets Assets { get; set; } = new();
}
