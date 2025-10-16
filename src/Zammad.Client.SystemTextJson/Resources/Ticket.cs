using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public class Ticket
{
    [JsonPropertyName("id")]
    public TicketId Id { get; set; }

    [JsonPropertyName("group_id")]
    public GroupId? GroupId { get; set; }

    [JsonPropertyName("priority_id")]
    public PriorityId? PriorityId { get; set; }

    [JsonPropertyName("state_id")]
    public StateId? StateId { get; set; }

    [JsonPropertyName("organization_id")]
    public OrganizationId? OrganizationId { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("owner_id")]
    public UserId? OwnerId { get; set; }

    [JsonPropertyName("customer_id")]
    public UserId? CustomerId { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("first_response_at")]
    public DateTimeOffset? FirstResponseAt { get; set; }

    [JsonPropertyName("first_response_escalation_at")]
    public DateTimeOffset? FirstResponseEscalationAt { get; set; }

    [JsonPropertyName("first_response_in_min")]
    public int? FirstResponseInMin { get; set; }

    [JsonPropertyName("first_response_diff_in_min")]
    public int? FirstResponseDiffInMin { get; set; }

    [JsonPropertyName("close_at")]
    public DateTimeOffset? CloseAt { get; set; }

    [JsonPropertyName("close_escalation_at")]
    public DateTimeOffset? CloseEscalationAt { get; set; }

    [JsonPropertyName("close_in_min")]
    public int? CloseInMin { get; set; }

    [JsonPropertyName("close_diff_in_min")]
    public int? CloseDiffInMin { get; set; }

    [JsonPropertyName("update_escalation_at")]
    public DateTimeOffset? UpdateEscalationAt { get; set; }

    [JsonPropertyName("update_in_min")]
    public int? UpdateInMin { get; set; }

    [JsonPropertyName("update_diff_in_min")]
    public int? UpdateDiffInMin { get; set; }

    [JsonPropertyName("last_close_at")]
    public DateTimeOffset? LastCloseAt { get; set; }

    [JsonPropertyName("last_contact_at")]
    public DateTimeOffset? LastContactAt { get; set; }

    [JsonPropertyName("last_contact_agent_at")]
    public DateTimeOffset? LastContactAgentAt { get; set; }

    [JsonPropertyName("last_contact_customer_at")]
    public DateTimeOffset? LastContactCustomerAt { get; set; }

    [JsonPropertyName("last_owner_update_at")]
    public DateTimeOffset? LastOwnerUpdateAt { get; set; }

    [JsonPropertyName("create_article_type_id")]
    public ArticleTypeId? CreateArticleTypeId { get; set; }

    [JsonPropertyName("create_article_sender_id")]
    public UserId? CreateArticleSenderId { get; set; }

    [JsonPropertyName("article_count")]
    public int? ArticleCount { get; set; }

    [JsonPropertyName("escalation_at")]
    public DateTimeOffset? EscalationAt { get; set; }

    [JsonPropertyName("pending_time")]
    public DateTimeOffset? PendingTime { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("time_unit")]
    public double? TimeUnit { get; set; }

    [JsonPropertyName("preferences")]
    public IDictionary<string, object>? Preferences { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
