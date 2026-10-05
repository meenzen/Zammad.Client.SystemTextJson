using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// The models the global search can search.
/// </summary>
public enum SearchObjectType
{
    Ticket,
    User,
    Organization,

    /// <summary>
    /// Knowledge base answers. Only searched if a knowledge base is active and the user has a
    /// <c>knowledge_base.*</c> permission.
    /// </summary>
    KnowledgeBaseAnswerTranslation,

    /// <summary>
    /// Chat sessions. Only searched if chat is enabled and the user has the <c>chat.agent</c> permission.
    /// </summary>
    ChatSession,
}

internal static class SearchObjectTypeExtensions
{
    /// <summary>
    /// The model name Zammad uses in search results and assets.
    /// </summary>
    internal static string ToModelName(this SearchObjectType type) =>
        type switch
        {
            SearchObjectType.Ticket => "Ticket",
            SearchObjectType.User => "User",
            SearchObjectType.Organization => "Organization",
            SearchObjectType.KnowledgeBaseAnswerTranslation => "KnowledgeBaseAnswerTranslation",
            SearchObjectType.ChatSession => "ChatSession",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
}

/// <summary>
/// One hit of the global search.
/// </summary>
public sealed class SearchHit
{
    /// <summary>
    /// Model name, e.g. <c>Ticket</c>, <c>User</c>, <c>Organization</c>, <c>KnowledgeBaseAnswerTranslation</c> or
    /// <c>ChatSession</c>.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public TargetObjectId Id { get; set; }

    [JsonIgnore]
    public TicketId? TicketId => Type == "Ticket" ? new TicketId(Id.Value) : null;

    [JsonIgnore]
    public UserId? UserId => Type == "User" ? new UserId(Id.Value) : null;

    [JsonIgnore]
    public OrganizationId? OrganizationId => Type == "Organization" ? new OrganizationId(Id.Value) : null;
}

/// <summary>
/// The result of the global search, as one list.
/// </summary>
public sealed class SearchResult
{
    /// <summary>
    /// The hits, grouped by model (tickets, users, organizations, knowledge base answers, chat sessions) and ordered
    /// by relevance or the requested sorting within each model.
    /// </summary>
    [JsonPropertyName("result")]
    public List<SearchHit> Hits { get; set; } = [];

    /// <summary>
    /// The found records and the records they reference.
    /// </summary>
    [JsonPropertyName("assets")]
    public Assets Assets { get; set; } = new();

    /// <summary>
    /// The tickets in <see cref="Hits"/>, resolved through <see cref="Assets"/>.
    /// </summary>
    public List<Ticket> GetTickets() => Assets.Resolve(Hits.Select(h => h.TicketId), id => Find(id, Assets.GetTicket));

    /// <summary>
    /// The users in <see cref="Hits"/>, resolved through <see cref="Assets"/>. Users that are only referenced by a
    /// found ticket are not included.
    /// </summary>
    public List<User> GetUsers() => Assets.Resolve(Hits.Select(h => h.UserId), id => Find(id, Assets.GetUser));

    /// <summary>
    /// The organizations in <see cref="Hits"/>, resolved through <see cref="Assets"/>.
    /// </summary>
    public List<Organization> GetOrganizations() =>
        Assets.Resolve(Hits.Select(h => h.OrganizationId), id => Find(id, Assets.GetOrganization));

    private static T? Find<TId, T>(TId? id, Func<TId, T?> lookup)
        where TId : struct
        where T : class => id is { } value ? lookup(value) : null;
}

/// <summary>
/// The result of the global search, grouped by model.
/// </summary>
public sealed class SearchResultByObject
{
    /// <summary>
    /// The hits keyed by model name (<c>Ticket</c>, <c>User</c>, ...). Models without hits may be missing.
    /// </summary>
    [JsonPropertyName("result")]
    public Dictionary<string, SearchObjectResult> Results { get; set; } = [];

    /// <summary>
    /// The found records and the records they reference.
    /// </summary>
    [JsonPropertyName("assets")]
    public Assets Assets { get; set; } = new();

    /// <summary>
    /// The hits of one model, or <c>null</c> if there are none.
    /// </summary>
    public SearchObjectResult? Get(SearchObjectType type) =>
        Results.TryGetValue(type.ToModelName(), out var result) ? result : null;

    /// <summary>
    /// The found tickets, resolved through <see cref="Assets"/>.
    /// </summary>
    public List<Ticket> GetTickets() =>
        Assets.Resolve(Ids(SearchObjectType.Ticket), id => Assets.GetTicket(new TicketId(id.Value)));

    /// <summary>
    /// The found users, resolved through <see cref="Assets"/>.
    /// </summary>
    public List<User> GetUsers() =>
        Assets.Resolve(Ids(SearchObjectType.User), id => Assets.GetUser(new UserId(id.Value)));

    /// <summary>
    /// The found organizations, resolved through <see cref="Assets"/>.
    /// </summary>
    public List<Organization> GetOrganizations() =>
        Assets.Resolve(Ids(SearchObjectType.Organization), id => Assets.GetOrganization(new OrganizationId(id.Value)));

    private List<TargetObjectId> Ids(SearchObjectType type) => Get(type)?.ObjectIds ?? [];
}

public sealed class SearchObjectResult
{
    /// <summary>
    /// The IDs of the hits on the requested page, in order.
    /// </summary>
    [JsonPropertyName("object_ids")]
    public List<TargetObjectId> ObjectIds { get; set; } = [];

    /// <summary>
    /// The number of hits of this model on all pages.
    /// </summary>
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
}
