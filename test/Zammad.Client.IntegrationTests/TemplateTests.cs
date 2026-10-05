using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TemplateTests(ZammadStackFixture zammadStack)
{
    private const string AgentLogin = "agent1@example.org";
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string TemplateName = "TestTemplate" + RandomName;
    private static TemplateId CreatedTemplateId { get; set; } = TemplateId.Empty;

    [Test]
    public async Task CreateTemplate()
    {
        var client = await zammadStack.GetClientAsync();

        var template = await client.CreateTemplateAsync(
            new Template
            {
                Name = TemplateName,
                Options = JsonSerializer.SerializeToElement(
                    new Dictionary<string, object>
                    {
                        ["ticket.title"] = new { value = "Template title" },
                        ["ticket.group_id"] = new { value = "1" },
                    }
                ),
                Active = true,
            }
        );

        await Assert.That(template.Id).IsNotEqualTo(TemplateId.Empty);
        await Assert.That(template.Name).IsEqualTo(TemplateName);
        await Assert.That(template.Active).IsTrue();
        await Assert
            .That(template.Options!.Value.GetProperty("ticket.title").GetProperty("value").GetString())
            .IsEqualTo("Template title");

        CreatedTemplateId = template.Id;
    }

    [Test]
    [DependsOn(nameof(CreateTemplate))]
    public async Task ListTemplates()
    {
        var client = await zammadStack.GetClientAsync();

        var templates = await client.ListTemplatesAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(templates).Contains(t => t.Id == CreatedTemplateId);
    }

    [Test]
    [DependsOn(nameof(CreateTemplate))]
    public async Task GetTemplate()
    {
        var client = await zammadStack.GetClientAsync();

        var template = await client.GetTemplateAsync(CreatedTemplateId);

        await Assert.That(template).IsNotNull();
        await Assert.That(template!.Name).IsEqualTo(TemplateName);
        await Assert
            .That(template.Options!.Value.GetProperty("ticket.group_id").GetProperty("value").GetString())
            .IsEqualTo("1");
    }

    [Test]
    [DependsOn(nameof(CreateTemplate))]
    public async Task GetTemplate_AsAgent()
    {
        var client = await zammadStack.GetClientOnBehalfOfAsync(AgentLogin);

        await Assert.That(await client.GetTemplateAsync(CreatedTemplateId)).IsNotNull();
    }

    [Test]
    [DependsOn(nameof(CreateTemplate))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchTemplates(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var templates = await client.SearchTemplatesAsync(new SearchQuery { Query = TemplateName });

        await Assert.That(templates).HasSingleItem();
        await Assert.That(templates[0].Id).IsEqualTo(CreatedTemplateId);
        await Assert.That(templates[0].CreatedBy).IsEqualTo("admin@example.org");
    }

    [Test]
    [DependsOn(nameof(ListTemplates))]
    [DependsOn(nameof(GetTemplate))]
    [DependsOn(nameof(GetTemplate_AsAgent))]
    [DependsOn(nameof(SearchTemplates))]
    public async Task UpdateTemplate()
    {
        var client = await zammadStack.GetClientAsync();

        var template = await client.GetTemplateAsync(CreatedTemplateId);
        await Assert.That(template).IsNotNull();
        template!.Name = TemplateName + "Updated";
        template.Active = false;

        var updated = await client.UpdateTemplateAsync(CreatedTemplateId, template);

        await Assert.That(updated.Name).IsEqualTo(TemplateName + "Updated");
        await Assert.That(updated.Active).IsFalse();
    }

    [Test]
    [DependsOn(nameof(UpdateTemplate))]
    public async Task GetTemplate_AsAgent_ReturnsNullForInactiveTemplate()
    {
        var client = await zammadStack.GetClientOnBehalfOfAsync(AgentLogin);

        await Assert.That(await client.GetTemplateAsync(CreatedTemplateId)).IsNull();
        await Assert.That(await client.ListTemplatesAsync()).DoesNotContain(t => t.Id == CreatedTemplateId);
    }

    [Test]
    [DependsOn(nameof(GetTemplate_AsAgent_ReturnsNullForInactiveTemplate))]
    public async Task DeleteTemplate()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTemplateAsync(CreatedTemplateId);
        await Assert.That(await client.GetTemplateAsync(CreatedTemplateId)).IsNull();
    }
}
