using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class CoreWorkflowTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string CoreWorkflowName = "Test Core Workflow " + RandomName;
    private static CoreWorkflowId CreatedCoreWorkflowId { get; set; } = CoreWorkflowId.Empty;

    [Test]
    public async Task CreateCoreWorkflow()
    {
        var client = await zammadStack.GetClientAsync();

        // Inactive: active workflows also validate API requests, so this one could break ticket tests
        var coreWorkflow = await client.CreateCoreWorkflowAsync(
            new CoreWorkflow
            {
                Name = CoreWorkflowName,
                Object = "Ticket",
                Active = false,
                Preferences = new Dictionary<string, JsonElement>
                {
                    ["screen"] = JsonSerializer.SerializeToElement(new[] { "create_middle", "edit" }),
                },
                ConditionSaved = JsonSerializer.Deserialize<JsonElement>(
                    $$$"""{"ticket.title":{"operator":"contains","value":"{{{RandomName}}}"}}"""
                ),
                Perform = JsonSerializer.Deserialize<JsonElement>(
                    """{"ticket.priority_id":{"operator":"remove_option","remove_option":["3"]}}"""
                ),
                Priority = 500,
            }
        );

        await Assert.That(coreWorkflow.Id).IsNotEqualTo(CoreWorkflowId.Empty);
        await Assert.That(coreWorkflow.Name).IsEqualTo(CoreWorkflowName);
        await Assert.That(coreWorkflow.Object).IsEqualTo("Ticket");
        await Assert.That(coreWorkflow.Active).IsFalse();
        await Assert.That(coreWorkflow.Changeable).IsTrue();
        await Assert.That(coreWorkflow.Priority).IsEqualTo(500);
        await Assert.That(coreWorkflow.Preferences!["screen"].GetArrayLength()).IsEqualTo(2);

        CreatedCoreWorkflowId = coreWorkflow.Id;
    }

    [Test]
    [DependsOn(nameof(CreateCoreWorkflow))]
    public async Task ListCoreWorkflows()
    {
        var client = await zammadStack.GetClientAsync();

        var coreWorkflows = await client.ListCoreWorkflowsAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(coreWorkflows).Contains(w => w.Id == CreatedCoreWorkflowId);
        // Zammad's built-in workflows aren't changeable and hidden from the API
        await Assert.That(coreWorkflows.All(w => w.Changeable == true)).IsTrue();
    }

    [Test]
    [DependsOn(nameof(CreateCoreWorkflow))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchCoreWorkflows(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var coreWorkflows = await client.SearchCoreWorkflowsAsync(new SearchQuery { Query = RandomName });

        await Assert.That(coreWorkflows).HasSingleItem();
        await Assert.That(coreWorkflows[0].Id).IsEqualTo(CreatedCoreWorkflowId);
    }

    [Test]
    [DependsOn(nameof(ListCoreWorkflows))]
    public async Task GetCoreWorkflow()
    {
        var client = await zammadStack.GetClientAsync();

        var coreWorkflow = await client.GetCoreWorkflowAsync(CreatedCoreWorkflowId);

        await Assert.That(coreWorkflow).IsNotNull();
        await Assert.That(coreWorkflow!.Name).IsEqualTo(CoreWorkflowName);
    }

    [Test]
    public async Task GetCoreWorkflow_ReturnsNullForBuiltInWorkflow()
    {
        var client = await zammadStack.GetClientAsync();

        // ID 1 is one of Zammad's seeded workflows, which aren't changeable
        await Assert.That(await client.GetCoreWorkflowAsync(new CoreWorkflowId(1))).IsNull();
    }

    [Test]
    [DependsOn(nameof(GetCoreWorkflow))]
    [DependsOn(nameof(SearchCoreWorkflows))]
    public async Task UpdateCoreWorkflow()
    {
        var client = await zammadStack.GetClientAsync();

        var coreWorkflow = await client.GetCoreWorkflowAsync(CreatedCoreWorkflowId);
        await Assert.That(coreWorkflow).IsNotNull();
        coreWorkflow!.StopAfterMatch = true;
        coreWorkflow.Priority = 501;

        var updated = await client.UpdateCoreWorkflowAsync(CreatedCoreWorkflowId, coreWorkflow);

        await Assert.That(updated.StopAfterMatch).IsTrue();
        await Assert.That(updated.Priority).IsEqualTo(501);
        await Assert.That(updated.Active).IsFalse();
    }

    [Test]
    [DependsOn(nameof(UpdateCoreWorkflow))]
    public async Task DeleteCoreWorkflow()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteCoreWorkflowAsync(CreatedCoreWorkflowId);

        await Assert.That(await client.GetCoreWorkflowAsync(CreatedCoreWorkflowId)).IsNull();
    }
}
