using System.Net;
using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class WebhookTests(ZammadStackFixture zammadStack)
{
    private const string Mask = "**********";

    // The stack has no internet access and Zammad resolves the host name on save, so use an IP. The webhook is
    // inactive and never called.
    private const string Endpoint = "http://127.0.0.1:9/webhook";

    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string WebhookName = "Test Webhook " + RandomName;
    private static WebhookId CreatedWebhookId { get; set; } = WebhookId.Empty;
    private static TriggerId ReferencingTriggerId { get; set; } = TriggerId.Empty;

    [Test]
    public async Task CreateWebhook()
    {
        var client = await zammadStack.GetClientAsync();

        var webhook = await client.CreateWebhookAsync(
            new Webhook
            {
                Name = WebhookName,
                Endpoint = Endpoint,
                Active = false,
                BearerToken = "secret-token",
                BasicAuthUsername = "user",
                BasicAuthPassword = "secret-password",
                Note = "created by integration test",
            }
        );

        await Assert.That(webhook.Id).IsNotEqualTo(WebhookId.Empty);
        await Assert.That(webhook.Name).IsEqualTo(WebhookName);
        await Assert.That(webhook.Endpoint).IsEqualTo(Endpoint);
        await Assert.That(webhook.HttpMethod).IsEqualTo("post");
        await Assert.That(webhook.Active).IsFalse();
        await Assert.That(webhook.BasicAuthUsername).IsEqualTo("user");
        await Assert.That(webhook.BearerToken).IsEqualTo(Mask);
        await Assert.That(webhook.BasicAuthPassword).IsEqualTo(Mask);
        await Assert.That(webhook.SignatureToken).IsNull();

        CreatedWebhookId = webhook.Id;
    }

    [Test]
    public async Task CreateWebhook_ThrowsForUnresolvableEndpoint()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateWebhookAsync(
                new Webhook
                {
                    Name = "Invalid Webhook " + RandomName,
                    Endpoint = "http://unresolvable.invalid/webhook",
                    Active = false,
                }
            )
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).Contains("could not be resolved");
    }

    [Test]
    [DependsOn(nameof(CreateWebhook))]
    public async Task ListWebhooks()
    {
        var client = await zammadStack.GetClientAsync();

        var webhooks = await client.ListWebhooksAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(webhooks).Contains(w => w.Id == CreatedWebhookId);
    }

    [Test]
    [DependsOn(nameof(CreateWebhook))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchWebhooks(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var webhooks = await client.SearchWebhooksAsync(new SearchQuery { Query = RandomName });

        await Assert.That(webhooks).HasSingleItem();
        await Assert.That(webhooks[0].Id).IsEqualTo(CreatedWebhookId);
    }

    [Test]
    [DependsOn(nameof(ListWebhooks))]
    public async Task GetWebhook()
    {
        var client = await zammadStack.GetClientAsync();

        var webhook = await client.GetWebhookAsync(CreatedWebhookId);

        await Assert.That(webhook).IsNotNull();
        await Assert.That(webhook!.Name).IsEqualTo(WebhookName);
        await Assert.That(webhook.BearerToken).IsEqualTo(Mask);
    }

    [Test]
    [DependsOn(nameof(GetWebhook))]
    [DependsOn(nameof(SearchWebhooks))]
    public async Task UpdateWebhook()
    {
        var client = await zammadStack.GetClientAsync();

        // The masked secrets are sent back unchanged, and Zammad keeps the stored values
        var webhook = await client.GetWebhookAsync(CreatedWebhookId);
        await Assert.That(webhook).IsNotNull();
        webhook!.Note = "updated";
        webhook.HttpMethod = "put";

        var updated = await client.UpdateWebhookAsync(CreatedWebhookId, webhook);

        await Assert.That(updated.Note).IsEqualTo("updated");
        await Assert.That(updated.HttpMethod).IsEqualTo("put");
        await Assert.That(updated.BearerToken).IsEqualTo(Mask);
        await Assert.That(updated.Active).IsFalse();
    }

    [Test]
    [DependsOn(nameof(UpdateWebhook))]
    public async Task DeleteWebhook_ThrowsWhileReferencedByTrigger()
    {
        var client = await zammadStack.GetClientAsync();

        var trigger = await client.CreateTriggerAsync(
            new Trigger
            {
                Name = "Webhook Trigger " + RandomName,
                Active = false,
                Condition = JsonSerializer.Deserialize<JsonElement>(
                    $$$"""{"ticket.title":{"operator":"contains","value":"{{{RandomName}}}"}}"""
                ),
                Perform = JsonSerializer.Deserialize<JsonElement>(
                    $$$"""{"notification.webhook":{"webhook_id":"{{{CreatedWebhookId}}}"}}"""
                ),
            }
        );
        ReferencingTriggerId = trigger.Id;

        var exception = await Assert.ThrowsAsync<ZammadException>(() => client.DeleteWebhookAsync(CreatedWebhookId));

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).StartsWith("This object is referenced by other object(s)");
        await Assert.That(exception.Content).Contains($"#{trigger.Id}");
    }

    [Test]
    [DependsOn(nameof(DeleteWebhook_ThrowsWhileReferencedByTrigger))]
    public async Task DeleteWebhook()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTriggerAsync(ReferencingTriggerId);
        await client.DeleteWebhookAsync(CreatedWebhookId);

        await Assert.That(await client.GetWebhookAsync(CreatedWebhookId)).IsNull();
    }

    [Test]
    public async Task ListPreDefinedWebhooks()
    {
        var client = await zammadStack.GetClientAsync();

        var preDefined = await client.ListPreDefinedWebhooksAsync();

        var mattermost = preDefined.SingleOrDefault(w => w.Id == "Mattermost");
        await Assert.That(mattermost).IsNotNull();
        await Assert.That(mattermost!.CustomPayload).Contains("#{ticket.title}");
        await Assert.That(mattermost.FieldNames).Contains("messaging_channel");
        await Assert.That(preDefined).Contains(w => w.Id == "Slack");
    }

    [Test]
    public async Task GetWebhookPayloadReplacements()
    {
        var client = await zammadStack.GetClientAsync();

        var replacements = await client.GetWebhookPayloadReplacementsAsync();

        await Assert.That(replacements["ticket"]).Contains("title");
        await Assert.That(replacements["notification"]).Contains("link");
        await Assert.That(replacements.ContainsKey("webhook")).IsFalse();
    }

    [Test]
    public async Task GetWebhookPayloadReplacements_PreDefinedType()
    {
        var client = await zammadStack.GetClientAsync();

        var replacements = await client.GetWebhookPayloadReplacementsAsync("Mattermost");

        await Assert.That(replacements["webhook"]).Contains("messaging_channel");
    }
}
