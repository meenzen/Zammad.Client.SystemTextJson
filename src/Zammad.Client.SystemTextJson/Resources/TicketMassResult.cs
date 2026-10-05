using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// The result of a mass update or mass macro.
/// </summary>
public sealed class TicketMassResult
{
    [JsonPropertyName("ticket_ids")]
    public List<TicketId> TicketIds { get; set; } = [];

    /// <summary>
    /// The updated tickets and the records they reference.
    /// </summary>
    [JsonPropertyName("assets")]
    public Assets Assets { get; set; } = new();

    /// <summary>
    /// The updated tickets from <see cref="Assets"/>, in the order of <see cref="TicketIds"/>.
    /// </summary>
    public List<Ticket> GetTickets() => Assets.ResolveTickets(TicketIds);
}
