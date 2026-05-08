using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TicketPriorityTests(ZammadStackFixture zammadStack)
{
    private static readonly string PriorityName = "TestPriority" + TestSetup.RandomString();
    private static PriorityId CreatedPriorityId { get; set; } = PriorityId.Empty;

    [Test]
    public async Task CreateTicketPriority()
    {
        var client = await zammadStack.GetClientAsync();

        var priority = await client.CreateTicketPriorityAsync(
            new TicketPriority { Name = PriorityName, Active = true }
        );

        await Assert.That(priority).IsNotNull();
        await Assert.That(priority.Id).IsNotEqualTo(PriorityId.Empty);
        await Assert.That(priority.Name).IsEqualTo(PriorityName);
        await Assert.That(priority.Active).IsTrue();

        CreatedPriorityId = priority.Id;
    }

    [Test]
    [DependsOn(nameof(CreateTicketPriority))]
    public async Task ListTicketPriorities()
    {
        var client = await zammadStack.GetClientAsync();

        var priorities = await client.ListTicketPrioritiesAsync();

        await Assert.That(priorities).HasAtLeast(1);
        await Assert.That(priorities).Contains(p => p.Id == CreatedPriorityId);
    }

    [Test]
    [DependsOn(nameof(CreateTicketPriority))]
    public async Task ListTicketPriorities_Pagination()
    {
        var client = await zammadStack.GetClientAsync();

        var priorities = await client.ListTicketPrioritiesAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(priorities).HasAtLeast(1);
        await Assert.That(priorities).Contains(p => p.Id == CreatedPriorityId);
    }

    [Test]
    [DependsOn(nameof(ListTicketPriorities))]
    [DependsOn(nameof(ListTicketPriorities_Pagination))]
    public async Task GetTicketPriority()
    {
        var client = await zammadStack.GetClientAsync();

        var priority = await client.GetTicketPriorityAsync(CreatedPriorityId);

        await Assert.That(priority).IsNotNull();
        await Assert.That(priority!.Id).IsEqualTo(CreatedPriorityId);
        await Assert.That(priority.Name).IsEqualTo(PriorityName);
        await Assert.That(priority.Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(GetTicketPriority))]
    public async Task UpdateTicketPriority()
    {
        var client = await zammadStack.GetClientAsync();

        var priority = await client.GetTicketPriorityAsync(CreatedPriorityId);
        await Assert.That(priority).IsNotNull();
        priority!.Note = "Updated note";

        var updated = await client.UpdateTicketPriorityAsync(CreatedPriorityId, priority);

        await Assert.That(updated.Note).IsEqualTo("Updated note");
    }

    [Test]
    [DependsOn(nameof(UpdateTicketPriority))]
    public async Task DeleteTicketPriority()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.DeleteTicketPriorityAsync(CreatedPriorityId);

        await Assert.That(result).IsTrue();
    }
}
