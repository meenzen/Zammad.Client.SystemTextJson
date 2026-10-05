using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class TicketTitleRequest
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }
}
