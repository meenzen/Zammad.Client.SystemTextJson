using System.Text.Json.Serialization;
using Riok.Mapperly.Abstractions;

namespace Zammad.Client.Resources.Internal;

#nullable enable

internal class TicketWithArticle : Ticket
{
    [JsonPropertyName("article")]
    public required TicketArticle Article { get; set; }
}

[Mapper]
internal static partial class TicketWithArticleExtensions
{
    internal static partial TicketWithArticle Combine(this Ticket ticket, TicketArticle article);
}
