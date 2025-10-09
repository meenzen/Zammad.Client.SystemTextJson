using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public class TicketArticle
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ticket_id")]
    public int TicketId { get; set; }

    [JsonPropertyName("sender_id")]
    public int? SenderId { get; set; }

    [JsonPropertyName("from")]
    public string From { get; set; }

    [JsonPropertyName("to")]
    public string To { get; set; }

    [JsonPropertyName("cc")]
    public string CC { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    [JsonPropertyName("reply_to")]
    public string ReplyTo { get; set; }

    [JsonPropertyName("message_id")]
    public string MessageId { get; set; }

    [JsonPropertyName("message_id_md5")]
    public string MessageIdMD5 { get; set; }

    [JsonPropertyName("in_reply_to")]
    public string InReplyTo { get; set; }

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }

    [JsonPropertyName("references")]
    public string References { get; set; }

    [JsonPropertyName("body")]
    public string Body { get; set; }

    [JsonPropertyName("internal")]
    public bool Internal { get; set; }

    [JsonPropertyName("preferences")]
    public IDictionary<string, object> Preferences { get; set; }

    [JsonPropertyName("updated_by_id")]
    public int? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public int? CreatedById { get; set; }

    [JsonPropertyName("origin_by_id")]
    public int? OriginById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("attachments")]
    public List<TicketAttachment> Attachments { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    [JsonPropertyName("created_by")]
    public string CreatedBy { get; set; }

    [JsonPropertyName("updated_by")]
    public string UpdatedBy { get; set; }
}
