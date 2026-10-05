using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

/// <summary>
/// Checklists of tickets, their items, and checklist templates.
/// </summary>
/// <remarks>
/// Needs the <c>checklist</c> setting, which is enabled by default. Reading and changing a checklist needs the same
/// permission on its ticket as an agent. Templates can be read by agents and changed by admins
/// (<c>admin.checklist</c>).
/// </remarks>
public interface IChecklistService
{
    /// <summary>
    /// Returns a checklist with its items. Find the checklist of a ticket with <see cref="Ticket.ChecklistId"/>.
    /// </summary>
    /// <remarks>
    /// Zammad responds with 403, not 404, if the checklist doesn't exist, so this throws
    /// <see cref="ZammadException"/> instead of returning <c>null</c>.
    /// </remarks>
    Task<Checklist?> GetChecklistAsync(ChecklistId id);

    /// <summary>
    /// Creates an empty checklist for a ticket.
    /// </summary>
    /// <param name="ticketId">The ticket. Needs change permission.</param>
    /// <param name="createFirstItem">Whether to add an item with an empty text, like the web UI does.</param>
    /// <remarks>
    /// Zammad responds with 422 "This ticket already has a checklist." if the ticket has one.
    /// <see cref="CreateTicketChecklistItemAsync"/> creates the checklist only if the ticket has none.
    /// </remarks>
    Task<Checklist> CreateChecklistAsync(TicketId ticketId, bool createFirstItem = false);

    /// <summary>
    /// Creates a checklist for a ticket with the name and the items of an active template.
    /// </summary>
    /// <remarks>
    /// Zammad responds with 422 "Checklist template must be active to use as a checklist starting point." if the
    /// template is inactive. Later changes to the template don't affect the checklist.
    /// </remarks>
    Task<Checklist> CreateChecklistFromTemplateAsync(TicketId ticketId, ChecklistTemplateId templateId);

    /// <summary>
    /// Renames a checklist or reorders its items. Only <see cref="Checklist.Name"/> and
    /// <see cref="Checklist.SortedItemIds"/> are sent, <c>null</c> keeps the current value.
    /// </summary>
    Task<Checklist> UpdateChecklistAsync(ChecklistId id, Checklist checklist);

    /// <summary>
    /// Deletes a checklist and its items.
    /// </summary>
    Task DeleteChecklistAsync(ChecklistId id);

    /// <summary>
    /// Returns a checklist item.
    /// </summary>
    /// <remarks>
    /// Zammad responds with 403, not 404, if the item doesn't exist, so this throws <see cref="ZammadException"/>
    /// instead of returning <c>null</c>.
    /// </remarks>
    Task<ChecklistItem?> GetChecklistItemAsync(ChecklistItemId id);

    /// <summary>
    /// Adds an item to the end of a checklist. Only <see cref="ChecklistItem.Text"/> and
    /// <see cref="ChecklistItem.Checked"/> are sent.
    /// </summary>
    /// <remarks>
    /// A checklist can have at most 100 items.
    /// </remarks>
    Task<ChecklistItem> CreateChecklistItemAsync(ChecklistId checklistId, ChecklistItem item);

    /// <summary>
    /// Adds an item to the checklist of a ticket, and creates the checklist if the ticket has none. Only
    /// <see cref="ChecklistItem.Text"/> and <see cref="ChecklistItem.Checked"/> are sent.
    /// </summary>
    Task<ChecklistItem> CreateTicketChecklistItemAsync(TicketId ticketId, ChecklistItem item);

    /// <summary>
    /// Adds several items to the end of a checklist. Only <see cref="ChecklistItem.Text"/> and
    /// <see cref="ChecklistItem.Checked"/> are sent.
    /// </summary>
    /// <returns>The IDs of the new items in the given order.</returns>
    Task<List<ChecklistItemId>> CreateChecklistItemsAsync(ChecklistId checklistId, IEnumerable<ChecklistItem> items);

    /// <summary>
    /// Adds several items to the checklist of a ticket, and creates the checklist if the ticket has none. Only
    /// <see cref="ChecklistItem.Text"/> and <see cref="ChecklistItem.Checked"/> are sent.
    /// </summary>
    /// <returns>The IDs of the new items in the given order.</returns>
    Task<List<ChecklistItemId>> CreateTicketChecklistItemsAsync(TicketId ticketId, IEnumerable<ChecklistItem> items);

    /// <summary>
    /// Changes the text of an item or checks it. Only <see cref="ChecklistItem.Text"/> and
    /// <see cref="ChecklistItem.Checked"/> are sent, <c>null</c> keeps the current value.
    /// </summary>
    Task<ChecklistItem> UpdateChecklistItemAsync(ChecklistItemId id, ChecklistItem item);

    /// <summary>
    /// Deletes a checklist item and removes it from <see cref="Checklist.SortedItemIds"/>.
    /// </summary>
    Task DeleteChecklistItemAsync(ChecklistItemId id);

    /// <summary>
    /// Lists the checklist templates with their items, including inactive ones.
    /// </summary>
    Task<List<ChecklistTemplate>> ListChecklistTemplatesAsync(Pagination? pagination = null);

    /// <summary>
    /// Returns a checklist template with its items.
    /// </summary>
    Task<ChecklistTemplate?> GetChecklistTemplateAsync(ChecklistTemplateId id);

    /// <summary>
    /// Creates a checklist template. <see cref="ChecklistTemplate.Name"/>, <see cref="ChecklistTemplate.Active"/>
    /// and <see cref="ChecklistTemplate.Items"/> are sent.
    /// </summary>
    /// <remarks>
    /// Zammad doesn't return the item texts when it saves a template, so this requests the template again to fill
    /// <see cref="ChecklistTemplate.Items"/>.
    /// </remarks>
    Task<ChecklistTemplate> CreateChecklistTemplateAsync(ChecklistTemplate template);

    /// <summary>
    /// Updates a checklist template. <see cref="ChecklistTemplate.Name"/>, <see cref="ChecklistTemplate.Active"/>
    /// and <see cref="ChecklistTemplate.Items"/> are sent, <c>null</c> keeps the current value.
    /// </summary>
    /// <remarks>
    /// Setting <see cref="ChecklistTemplate.Items"/> replaces all items. Like
    /// <see cref="CreateChecklistTemplateAsync"/>, this requests the template again to fill the items.
    /// </remarks>
    Task<ChecklistTemplate> UpdateChecklistTemplateAsync(ChecklistTemplateId id, ChecklistTemplate template);

    /// <summary>
    /// Deletes a checklist template. Checklists created from it are kept.
    /// </summary>
    Task DeleteChecklistTemplateAsync(ChecklistTemplateId id);
}

public sealed partial class ZammadClient : IChecklistService
{
    private const string ChecklistsEndpoint = "/api/v1/checklists";
    private const string ChecklistItemsEndpoint = "/api/v1/checklist_items";
    private const string ChecklistTemplatesEndpoint = "/api/v1/checklist_templates";
    private const string FullQuery = "?full=true";

    public async Task<Checklist?> GetChecklistAsync(ChecklistId id)
    {
        var response = await GetAsync<FullResponse>($"{ChecklistsEndpoint}/{id}", FullQuery);
        return response is null ? null : GetChecklistWithItems(response.Assets, id);
    }

    public async Task<Checklist> CreateChecklistAsync(TicketId ticketId, bool createFirstItem = false) =>
        await CreateChecklistAsync(
            new ChecklistCreateRequest { TicketId = ticketId, CreateFirstItem = createFirstItem ? true : null }
        );

    public async Task<Checklist> CreateChecklistFromTemplateAsync(TicketId ticketId, ChecklistTemplateId templateId) =>
        await CreateChecklistAsync(new ChecklistCreateRequest { TicketId = ticketId, TemplateId = templateId });

    private async Task<Checklist> CreateChecklistAsync(ChecklistCreateRequest request)
    {
        var response =
            await PostAsync<FullResponse>(ChecklistsEndpoint, request) ?? throw LogicException.UnexpectedNullResult;
        if (response.Id is not { } id)
        {
            throw LogicException.UnexpectedNullResult;
        }

        return GetChecklistWithItems(response.Assets, new ChecklistId(id));
    }

    public async Task<Checklist> UpdateChecklistAsync(ChecklistId id, Checklist checklist)
    {
        var response =
            await PutAsync<FullResponse>(
                $"{ChecklistsEndpoint}/{id}{FullQuery}",
                new ChecklistUpdateRequest { Name = checklist.Name, SortedItemIds = checklist.SortedItemIds }
            ) ?? throw LogicException.UnexpectedNullResult;
        return GetChecklistWithItems(response.Assets, id);
    }

    public async Task DeleteChecklistAsync(ChecklistId id) => await DeleteAsync<bool>($"{ChecklistsEndpoint}/{id}");

    public async Task<ChecklistItem?> GetChecklistItemAsync(ChecklistItemId id) =>
        await GetAsync<ChecklistItem>($"{ChecklistItemsEndpoint}/{id}");

    public async Task<ChecklistItem> CreateChecklistItemAsync(ChecklistId checklistId, ChecklistItem item) =>
        await PostAsync<ChecklistItem>(
            ChecklistItemsEndpoint,
            new ChecklistItemRequest
            {
                ChecklistId = checklistId,
                Text = item.Text,
                Checked = item.Checked,
            }
        ) ?? throw LogicException.UnexpectedNullResult;

    public async Task<ChecklistItem> CreateTicketChecklistItemAsync(TicketId ticketId, ChecklistItem item) =>
        await PostAsync<ChecklistItem>(
            ChecklistItemsEndpoint,
            new ChecklistItemRequest
            {
                TicketId = ticketId,
                Text = item.Text,
                Checked = item.Checked,
            }
        ) ?? throw LogicException.UnexpectedNullResult;

    public async Task<List<ChecklistItemId>> CreateChecklistItemsAsync(
        ChecklistId checklistId,
        IEnumerable<ChecklistItem> items
    ) =>
        await CreateChecklistItemsAsync(
            new ChecklistItemBulkRequest { ChecklistId = checklistId, Items = ToBulk(items) }
        );

    public async Task<List<ChecklistItemId>> CreateTicketChecklistItemsAsync(
        TicketId ticketId,
        IEnumerable<ChecklistItem> items
    ) => await CreateChecklistItemsAsync(new ChecklistItemBulkRequest { TicketId = ticketId, Items = ToBulk(items) });

    private static List<ChecklistItemRequest> ToBulk(IEnumerable<ChecklistItem> items) =>
        items.Select(item => new ChecklistItemRequest { Text = item.Text, Checked = item.Checked }).ToList();

    private async Task<List<ChecklistItemId>> CreateChecklistItemsAsync(ChecklistItemBulkRequest request)
    {
        var response =
            await PostAsync<ChecklistItemBulkResponse>($"{ChecklistItemsEndpoint}/create_bulk", request)
            ?? throw LogicException.UnexpectedNullResult;
        return response.ChecklistItemIds ?? throw LogicException.UnexpectedNullResult;
    }

    public async Task<ChecklistItem> UpdateChecklistItemAsync(ChecklistItemId id, ChecklistItem item) =>
        await PutAsync<ChecklistItem>(
            $"{ChecklistItemsEndpoint}/{id}",
            new ChecklistItemRequest { Text = item.Text, Checked = item.Checked }
        ) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteChecklistItemAsync(ChecklistItemId id) =>
        await DeleteAsync<bool>($"{ChecklistItemsEndpoint}/{id}");

    public async Task<List<ChecklistTemplate>> ListChecklistTemplatesAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        builder.Add("full", true);
        var response = await GetAsync<FullResponse>(ChecklistTemplatesEndpoint, builder.ToString());
        if (response is null)
        {
            return [];
        }

        return (response.RecordIds ?? [])
            .Select(id => GetChecklistTemplateWithItems(response.Assets, new ChecklistTemplateId(id)))
            .ToList();
    }

    public async Task<ChecklistTemplate?> GetChecklistTemplateAsync(ChecklistTemplateId id)
    {
        var response = await GetAsync<FullResponse>($"{ChecklistTemplatesEndpoint}/{id}", FullQuery);
        return response is null ? null : GetChecklistTemplateWithItems(response.Assets, id);
    }

    public async Task<ChecklistTemplate> CreateChecklistTemplateAsync(ChecklistTemplate template)
    {
        var created =
            await PostAsync<ChecklistTemplate>(ChecklistTemplatesEndpoint, ToRequest(template))
            ?? throw LogicException.UnexpectedNullResult;
        return await GetChecklistTemplateAsync(created.Id) ?? throw LogicException.UnexpectedNullResult;
    }

    public async Task<ChecklistTemplate> UpdateChecklistTemplateAsync(
        ChecklistTemplateId id,
        ChecklistTemplate template
    )
    {
        await PutAsync<ChecklistTemplate>($"{ChecklistTemplatesEndpoint}/{id}", ToRequest(template));
        return await GetChecklistTemplateAsync(id) ?? throw LogicException.UnexpectedNullResult;
    }

    private static ChecklistTemplateRequest ToRequest(ChecklistTemplate template) =>
        new()
        {
            Name = template.Name,
            Active = template.Active,
            Items = template.Items,
        };

    public async Task DeleteChecklistTemplateAsync(ChecklistTemplateId id) =>
        await DeleteAsync<bool>($"{ChecklistTemplatesEndpoint}/{id}");

    private static Checklist GetChecklistWithItems(Assets assets, ChecklistId id)
    {
        var checklist = assets.GetChecklist(id) ?? throw LogicException.UnexpectedNullResult;
        var items = assets.ChecklistItems.Values.Where(item => item.ChecklistId == id).ToList();
        checklist.Items = SortBy(items, item => item.Id, checklist.SortedItemIds);
        return checklist;
    }

    private static ChecklistTemplate GetChecklistTemplateWithItems(Assets assets, ChecklistTemplateId id)
    {
        var template = assets.GetChecklistTemplate(id) ?? throw LogicException.UnexpectedNullResult;
        var items = assets.ChecklistTemplateItems.Values.Where(item => item.ChecklistTemplateId == id).ToList();
        template.Items = SortBy(items, item => item.Id, template.SortedItemIds)
            .Select(item => item.Text ?? string.Empty)
            .ToList();
        return template;
    }

    /// <summary>
    /// Sorts the items in the order of <paramref name="sortedIds"/>. Items that aren't in it come last, by ID.
    /// </summary>
    private static List<T> SortBy<T, TId>(List<T> items, Func<T, TId> getId, List<TId>? sortedIds)
        where TId : struct, IComparable<TId>
    {
        var positions = (sortedIds ?? [])
            .Distinct()
            .Select((id, position) => (id, position))
            .ToDictionary(pair => pair.id, pair => pair.position);

        return items
            .OrderBy(item => positions.TryGetValue(getId(item), out var position) ? position : int.MaxValue)
            .ThenBy(getId)
            .ToList();
    }
}
