using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

/// <summary>
/// <c>PUT /ticket_merge</c> answers 200 OK both on success (<c>result: "success"</c>) and when a ticket wasn't
/// found (<c>result: "failed"</c> with <c>message</c>).
/// </summary>
internal sealed class TicketMergeResponse
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("source_ticket")]
    public Ticket? SourceTicket { get; set; }

    [JsonPropertyName("target_ticket")]
    public Ticket? TargetTicket { get; set; }
}
