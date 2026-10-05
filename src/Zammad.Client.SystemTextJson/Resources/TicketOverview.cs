using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// An overview the current user can see, with its ticket count.
/// </summary>
public sealed class TicketOverviewSummary
{
    [JsonPropertyName("id")]
    public OverviewId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("prio")]
    public int? Prio { get; set; }

    /// <summary>
    /// The URL slug, used to get the tickets with <see cref="ITicketOverviewService.GetTicketOverviewAsync"/>.
    /// </summary>
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    /// <summary>
    /// The number of tickets in the overview for the current user.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

/// <summary>
/// The tickets of an overview for the current user.
/// </summary>
public sealed class TicketOverviewResult
{
    [JsonPropertyName("index")]
    public TicketOverviewIndex Index { get; set; } = new();

    /// <summary>
    /// The overview, the tickets and the records they reference.
    /// </summary>
    [JsonPropertyName("assets")]
    public Assets Assets { get; set; } = new();

    /// <summary>
    /// The tickets from <see cref="Assets"/>, in the order of the overview.
    /// </summary>
    public List<Ticket> GetTickets() => Assets.ResolveTickets(Index.Tickets.Select(t => t.Id));
}

public sealed class TicketOverviewIndex
{
    /// <summary>
    /// <c>null</c> if there is no overview with the requested link that the current user can see.
    /// </summary>
    [JsonPropertyName("overview")]
    public TicketOverviewReference? Overview { get; set; }

    /// <summary>
    /// The tickets in the overview's order, at most as many as the <c>ui_ticket_overview_ticket_limit</c> setting
    /// (default 2000).
    /// </summary>
    [JsonPropertyName("tickets")]
    public List<TicketOverviewTicket> Tickets { get; set; } = [];

    /// <summary>
    /// The number of all tickets in the overview, also those beyond the limit.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public sealed class TicketOverviewReference
{
    [JsonPropertyName("id")]
    public OverviewId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The overview's link.
    /// </summary>
    [JsonPropertyName("view")]
    public string? View { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class TicketOverviewTicket
{
    [JsonPropertyName("id")]
    public TicketId Id { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
