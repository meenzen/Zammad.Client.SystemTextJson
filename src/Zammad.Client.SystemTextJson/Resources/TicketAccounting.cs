using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class TicketAccounting
{
    [JsonPropertyName("id")]
    public TimeAccountingId Id { get; set; }

    [JsonPropertyName("ticket_id")]
    public TicketId? TicketId { get; set; }

    [JsonPropertyName("ticket_article_id")]
    public ArticleId? TicketArticleId { get; set; }

    [JsonPropertyName("time_unit")]
    public string? TimeUnit { get; set; }

    [JsonPropertyName("type_id")]
    public TimeAccountingTypeId? TypeId { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
