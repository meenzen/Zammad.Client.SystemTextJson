using System.Text.Json.Serialization;
using Riok.Mapperly.Abstractions;

namespace Zammad.Client.Resources.Internal;

#nullable enable

public class TicketWithArticle : Ticket
{
    [JsonPropertyName("article")]
    public required TicketArticle Article { get; set; }
}

[Mapper]
public static partial class TicketWithArticleExtensions
{
    public static partial TicketWithArticle Combine(this Ticket ticket, TicketArticle article);
}
