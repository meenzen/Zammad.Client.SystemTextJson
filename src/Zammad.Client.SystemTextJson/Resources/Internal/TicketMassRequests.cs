using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class TicketMassUpdateRequest
{
    [JsonPropertyName("ticket_ids")]
    public required IReadOnlyCollection<TicketId> TicketIds { get; set; }

    [JsonPropertyName("attributes")]
    public Ticket? Attributes { get; set; }

    [JsonPropertyName("article")]
    public TicketArticle? Article { get; set; }
}

internal sealed class TicketMassMacroRequest
{
    [JsonPropertyName("macro_id")]
    public required MacroId MacroId { get; set; }

    [JsonPropertyName("ticket_ids")]
    public required IReadOnlyCollection<TicketId> TicketIds { get; set; }
}
