using System.Net;
using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TriggerTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string TriggerName = "Test Trigger " + RandomName;
    private static TriggerId CreatedTriggerId { get; set; } = TriggerId.Empty;

    // Inactive and matching no ticket, so it never changes tickets of other tests
    private static JsonElement Condition =>
        JsonSerializer.Deserialize<JsonElement>(
            $$$"""{"ticket.title":{"operator":"contains","value":"{{{RandomName}}}"}}"""
        );

    [Test]
    public async Task CreateTrigger()
    {
        var client = await zammadStack.GetClientAsync();

        var trigger = await client.CreateTriggerAsync(
            new Trigger
            {
                Name = TriggerName,
                Active = false,
                Condition = Condition,
                Perform = JsonSerializer.Deserialize<JsonElement>("""{"ticket.priority_id":{"value":"3"}}"""),
                Note = "created by integration test",
            }
        );

        await Assert.That(trigger.Id).IsNotEqualTo(TriggerId.Empty);
        await Assert.That(trigger.Name).IsEqualTo(TriggerName);
        await Assert.That(trigger.Active).IsFalse();
        await Assert.That(trigger.Activator).IsEqualTo("action");
        await Assert.That(trigger.ExecutionConditionMode).IsEqualTo("selective");
        await Assert
            .That(trigger.Condition!.Value.GetProperty("ticket.title").GetProperty("value").GetString())
            .IsEqualTo(RandomName);

        CreatedTriggerId = trigger.Id;
    }

    [Test]
    public async Task CreateTrigger_ThrowsWithoutCondition()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateTriggerAsync(new Trigger { Name = "Invalid Trigger " + RandomName, Active = false })
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).Contains("condition");
    }

    [Test]
    [DependsOn(nameof(CreateTrigger))]
    public async Task ListTriggers()
    {
        var client = await zammadStack.GetClientAsync();

        var triggers = await client.ListTriggersAsync();

        await Assert.That(triggers).Contains(t => t.Id == CreatedTriggerId);
    }

    [Test]
    [DependsOn(nameof(CreateTrigger))]
    public async Task ListTriggers_Pagination()
    {
        var client = await zammadStack.GetClientAsync();

        var firstPage = await client.ListTriggersAsync(new Pagination { Page = 1, PerPage = 1 });

        await Assert.That(firstPage).HasSingleItem();
    }

    [Test]
    [DependsOn(nameof(CreateTrigger))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchTriggers(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var triggers = await client.SearchTriggersAsync(new SearchQuery { Query = RandomName });

        await Assert.That(triggers).HasSingleItem();
        await Assert.That(triggers[0].Id).IsEqualTo(CreatedTriggerId);
    }

    [Test]
    [DependsOn(nameof(ListTriggers))]
    [DependsOn(nameof(ListTriggers_Pagination))]
    public async Task GetTrigger()
    {
        var client = await zammadStack.GetClientAsync();

        var trigger = await client.GetTriggerAsync(CreatedTriggerId);

        await Assert.That(trigger).IsNotNull();
        await Assert.That(trigger!.Name).IsEqualTo(TriggerName);
        await Assert.That(trigger.Note).IsEqualTo("created by integration test");
    }

    [Test]
    [DependsOn(nameof(GetTrigger))]
    [DependsOn(nameof(SearchTriggers))]
    public async Task UpdateTrigger()
    {
        var client = await zammadStack.GetClientAsync();

        var trigger = await client.GetTriggerAsync(CreatedTriggerId);
        await Assert.That(trigger).IsNotNull();
        trigger!.Note = "updated";
        trigger.ExecutionConditionMode = "always";

        var updated = await client.UpdateTriggerAsync(CreatedTriggerId, trigger);

        await Assert.That(updated.Note).IsEqualTo("updated");
        await Assert.That(updated.ExecutionConditionMode).IsEqualTo("always");
        await Assert.That(updated.Active).IsFalse();
    }

    [Test]
    [DependsOn(nameof(UpdateTrigger))]
    public async Task DeleteTrigger()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTriggerAsync(CreatedTriggerId);

        await Assert.That(await client.GetTriggerAsync(CreatedTriggerId)).IsNull();
    }
}
