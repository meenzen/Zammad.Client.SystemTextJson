using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// Webhooks (<c>/api/v1/webhooks</c>), which triggers and schedulers call with the <c>notification.webhook</c>
/// perform action. Requires the <c>admin.webhook</c> permission.
/// </summary>
/// <remarks>
/// Zammad masks <see cref="Webhook.SignatureToken"/>, <see cref="Webhook.BearerToken"/> and
/// <see cref="Webhook.BasicAuthPassword"/> as <c>**********</c> in every response. Sending the mask back in an
/// update keeps the stored value, so a webhook can be fetched, changed and updated without losing its secrets.
/// </remarks>
public interface IWebhookService
{
    Task<List<Webhook>> ListWebhooksAsync(Pagination? pagination = null);

    /// <summary>
    /// Searches webhooks by name, endpoint and note.
    /// </summary>
    /// <remarks>
    /// With a query, Zammad searches the Elasticsearch index, so new or changed webhooks show up with a delay.
    /// </remarks>
    Task<List<Webhook>> SearchWebhooksAsync(SearchQuery query);

    Task<Webhook?> GetWebhookAsync(WebhookId id);

    /// <summary>
    /// Creates a webhook.
    /// </summary>
    /// <remarks>
    /// <see cref="Webhook.Name"/> and an http(s) <see cref="Webhook.Endpoint"/> are required. Zammad resolves the
    /// endpoint's host name while saving and responds with 422 if it can't be resolved.
    /// </remarks>
    Task<Webhook> CreateWebhookAsync(Webhook webhook);

    Task<Webhook> UpdateWebhookAsync(WebhookId id, Webhook webhook);

    /// <summary>
    /// Deletes a webhook.
    /// </summary>
    /// <remarks>
    /// Fails with 422 while a trigger, scheduler or other automation uses the webhook in its perform block. Zammad
    /// doesn't fill in the message ("... thus cannot be deleted: %s"); the referencing records are only listed in
    /// the <c>unprocessable_content</c> field of <see cref="ZammadException.Content"/>.
    /// </remarks>
    Task DeleteWebhookAsync(WebhookId id);

    /// <summary>
    /// Returns the pre-defined webhook types (e.g. Slack, Mattermost) for <see cref="Webhook.PreDefinedWebhookType"/>.
    /// </summary>
    Task<List<PreDefinedWebhook>> ListPreDefinedWebhooksAsync();

    /// <summary>
    /// Returns the variables that can be used in <see cref="Webhook.CustomPayload"/>, e.g.
    /// <c>#{ticket.title}</c>, grouped by object.
    /// </summary>
    /// <param name="preDefinedWebhookType">
    /// Includes the <c>webhook.*</c> variables of this pre-defined webhook type (<see cref="PreDefinedWebhook.Id"/>).
    /// </param>
    Task<Dictionary<string, List<string>>> GetWebhookPayloadReplacementsAsync(string? preDefinedWebhookType = null);
}

public sealed partial class ZammadClient : IWebhookService
{
    private const string WebhooksEndpoint = "/api/v1/webhooks";
    private const string WebhooksSearchEndpoint = "/api/v1/webhooks/search";

    public async Task<List<Webhook>> ListWebhooksAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Webhook>>(WebhooksEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Webhook>> SearchWebhooksAsync(SearchQuery query)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        return await GetAsync<List<Webhook>>(WebhooksSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Webhook?> GetWebhookAsync(WebhookId id) => await GetAsync<Webhook>($"{WebhooksEndpoint}/{id}");

    public async Task<Webhook> CreateWebhookAsync(Webhook webhook) =>
        await PostAsync<Webhook>(WebhooksEndpoint, webhook) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Webhook> UpdateWebhookAsync(WebhookId id, Webhook webhook) =>
        await PutAsync<Webhook>($"{WebhooksEndpoint}/{id}", webhook) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteWebhookAsync(WebhookId id) => await DeleteAsync<bool>($"{WebhooksEndpoint}/{id}");

    public async Task<List<PreDefinedWebhook>> ListPreDefinedWebhooksAsync() =>
        await GetAsync<List<PreDefinedWebhook>>($"{WebhooksEndpoint}/pre_defined") ?? [];

    public async Task<Dictionary<string, List<string>>> GetWebhookPayloadReplacementsAsync(
        string? preDefinedWebhookType = null
    )
    {
        var builder = new QueryBuilder();
        if (preDefinedWebhookType is { } type && !string.IsNullOrWhiteSpace(type))
        {
            builder.Add("pre_defined_webhook_type", type);
        }

        return await GetAsync<Dictionary<string, List<string>>>(
                $"{WebhooksEndpoint}/payload/replacements",
                builder.ToString()
            ) ?? throw LogicException.UnexpectedNullResult;
    }
}
