using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class Webhook
{
    [JsonPropertyName("id")]
    public WebhookId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The http(s) URL to call. It can contain variables such as <c>#{ticket.id}</c>.
    /// </summary>
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    /// <summary>
    /// <c>post</c> (default), <c>put</c>, <c>patch</c> or <c>delete</c>.
    /// </summary>
    [JsonPropertyName("http_method")]
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Secret for the <c>X-Hub-Signature</c> header. Masked as <c>**********</c> in responses.
    /// </summary>
    [JsonPropertyName("signature_token")]
    public string? SignatureToken { get; set; }

    [JsonPropertyName("ssl_verify")]
    public bool? SslVerify { get; set; }

    [JsonPropertyName("basic_auth_username")]
    public string? BasicAuthUsername { get; set; }

    /// <summary>
    /// Masked as <c>**********</c> in responses.
    /// </summary>
    [JsonPropertyName("basic_auth_password")]
    public string? BasicAuthPassword { get; set; }

    /// <summary>
    /// Masked as <c>**********</c> in responses.
    /// </summary>
    [JsonPropertyName("bearer_token")]
    public string? BearerToken { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// The <see cref="PreDefinedWebhook.Id"/> (e.g. <c>Slack</c>) if this webhook uses a pre-defined payload.
    /// </summary>
    [JsonPropertyName("pre_defined_webhook_type")]
    public string? PreDefinedWebhookType { get; set; }

    /// <summary>
    /// Whether <see cref="CustomPayload"/> is used. If not, Zammad sends its default payload and discards
    /// <see cref="CustomPayload"/> on save.
    /// </summary>
    [JsonPropertyName("customized_payload")]
    public bool? CustomizedPayload { get; set; }

    /// <summary>
    /// A JSON document as a string, with variables such as <c>#{ticket.title}</c> (see
    /// <see cref="IWebhookService.GetWebhookPayloadReplacementsAsync"/>).
    /// </summary>
    [JsonPropertyName("custom_payload")]
    public string? CustomPayload { get; set; }

    /// <summary>
    /// Settings of pre-defined webhooks, e.g. <c>{"pre_defined_webhook": {"messaging_channel": "#support"}}</c>.
    /// </summary>
    [JsonPropertyName("preferences")]
    public Dictionary<string, JsonElement>? Preferences { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
