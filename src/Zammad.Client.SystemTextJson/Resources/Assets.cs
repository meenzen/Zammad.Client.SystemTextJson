using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// Related records that Zammad sends along with some responses, so that clients don't have to look them up one by one.
/// </summary>
/// <remarks>
/// <para>
/// Zammad groups the records by model name (<c>Ticket</c>, <c>TicketArticle</c>, <c>User</c>, ...) and keys them by
/// their ID. The dictionaries are keyed by the plain <see cref="int"/> ID because the strongly typed IDs can't be used
/// as dictionary keys on netstandard2.0. Use the <c>Get*</c> methods to look records up by their typed ID.
/// </para>
/// <para>
/// The records contain the same attributes as the regular endpoints without <c>expand</c>. Zammad leaves out
/// records the current user isn't allowed to see. Models without a typed property end up in
/// <see cref="ExtensionData"/>.
/// </para>
/// </remarks>
public sealed class Assets
{
    [JsonPropertyName("Ticket")]
    public Dictionary<int, Ticket> Tickets { get; set; } = [];

    [JsonPropertyName("TicketArticle")]
    public Dictionary<int, TicketArticle> TicketArticles { get; set; } = [];

    [JsonPropertyName("User")]
    public Dictionary<int, User> Users { get; set; } = [];

    [JsonPropertyName("Organization")]
    public Dictionary<int, Organization> Organizations { get; set; } = [];

    [JsonPropertyName("Group")]
    public Dictionary<int, Group> Groups { get; set; } = [];

    [JsonPropertyName("Checklist")]
    public Dictionary<int, Checklist> Checklists { get; set; } = [];

    [JsonPropertyName("ChecklistItem")]
    public Dictionary<int, ChecklistItem> ChecklistItems { get; set; } = [];

    [JsonPropertyName("ChecklistTemplate")]
    public Dictionary<int, ChecklistTemplate> ChecklistTemplates { get; set; } = [];

    [JsonPropertyName("ChecklistTemplateItem")]
    public Dictionary<int, ChecklistTemplateItem> ChecklistTemplateItems { get; set; } = [];

    [JsonPropertyName("Overview")]
    public Dictionary<int, Overview> Overviews { get; set; } = [];

    /// <summary>
    /// Records of all other models, e.g. <c>Role</c>, <c>TicketState</c> or <c>TicketPriority</c>, keyed by the model
    /// name. Each value is an object that maps IDs to records.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public Ticket? GetTicket(TicketId id) => Find(Tickets, id.Value);

    public TicketArticle? GetTicketArticle(ArticleId id) => Find(TicketArticles, id.Value);

    public User? GetUser(UserId id) => Find(Users, id.Value);

    public Organization? GetOrganization(OrganizationId id) => Find(Organizations, id.Value);

    public Group? GetGroup(GroupId id) => Find(Groups, id.Value);

    public Checklist? GetChecklist(ChecklistId id) => Find(Checklists, id.Value);

    public ChecklistItem? GetChecklistItem(ChecklistItemId id) => Find(ChecklistItems, id.Value);

    public ChecklistTemplate? GetChecklistTemplate(ChecklistTemplateId id) => Find(ChecklistTemplates, id.Value);

    public ChecklistTemplateItem? GetChecklistTemplateItem(ChecklistTemplateItemId id) =>
        Find(ChecklistTemplateItems, id.Value);

    public Overview? GetOverview(OverviewId id) => Find(Overviews, id.Value);

    /// <summary>
    /// Looks up the given tickets and leaves out the ones that aren't in the assets.
    /// </summary>
    internal List<Ticket> ResolveTickets(IEnumerable<TicketId> ids) => Resolve(ids, GetTicket);

    internal static List<T> Resolve<TId, T>(IEnumerable<TId> ids, Func<TId, T?> lookup)
        where T : class
    {
        var records = new List<T>();
        foreach (var id in ids)
        {
            if (lookup(id) is { } record)
            {
                records.Add(record);
            }
        }

        return records;
    }

    private static T? Find<T>(Dictionary<int, T>? records, int id)
        where T : class => records is not null && records.TryGetValue(id, out var record) ? record : null;
}
