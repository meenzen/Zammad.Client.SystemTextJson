using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class TicketMergeResult
{
    /// <summary>
    /// The merged ticket after the merge, in the <c>merged</c> state and with the owner reset to the system user (ID 1).
    /// </summary>
    /// <remarks>
    /// Zammad returns the plain database columns, so association IDs such as <see cref="Ticket.ArticleIds"/> are not
    /// set.
    /// </remarks>
    [JsonPropertyName("source_ticket")]
    public Ticket? SourceTicket { get; set; }

    /// <summary>
    /// The ticket that was merged into, as it was loaded <b>before</b> the merge: the moved articles and the new
    /// <c>updated_at</c> aren't in it yet. Get the ticket again for its current state.
    /// </summary>
    [JsonPropertyName("target_ticket")]
    public Ticket? TargetTicket { get; set; }
}
