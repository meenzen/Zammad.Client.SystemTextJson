using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TicketStateTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string StateName = "TestState" + RandomName;
    private static StateId CreatedStateId { get; set; } = StateId.Empty;

    [Test]
    public async Task CreateTicketState()
    {
        var client = await zammadStack.GetClientAsync();

        var state = await client.CreateTicketStateAsync(
            new TicketState
            {
                Name = StateName,
                StateTypeId = new StateTypeId(1),
                Active = true,
            }
        );

        await Assert.That(state).IsNotNull();
        await Assert.That(state.Id).IsNotEqualTo(StateId.Empty);
        await Assert.That(state.Name).IsEqualTo(StateName);
        await Assert.That(state.Active).IsTrue();

        CreatedStateId = state.Id;
    }

    [Test]
    [DependsOn(nameof(CreateTicketState))]
    public async Task ListTicketStates()
    {
        var client = await zammadStack.GetClientAsync();

        var states = await client.ListTicketStatesAsync();

        await Assert.That(states).HasAtLeast(1);
        await Assert.That(states).Contains(s => s.Id == CreatedStateId);
    }

    [Test]
    [DependsOn(nameof(CreateTicketState))]
    public async Task ListTicketStates_Pagination()
    {
        var client = await zammadStack.GetClientAsync();

        var states = await client.ListTicketStatesAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(states).HasAtLeast(1);
        await Assert.That(states).Contains(s => s.Id == CreatedStateId);
    }

    [Test]
    [DependsOn(nameof(CreateTicketState))]
    [Obsolete("Testing legacy pagination.")]
    public async Task ListTicketStates_Pagination_Legacy()
    {
        var client = await zammadStack.GetClientAsync();

        var states = await client.ListTicketStatesAsync(1, 100);

        await Assert.That(states).HasAtLeast(1);
        await Assert.That(states).Contains(s => s.Id == CreatedStateId);
    }

    [Test]
    [DependsOn(nameof(ListTicketStates))]
    [DependsOn(nameof(ListTicketStates_Pagination))]
    [DependsOn(nameof(ListTicketStates_Pagination_Legacy))]
    public async Task GetTicketState()
    {
        var client = await zammadStack.GetClientAsync();

        var state = await client.GetTicketStateAsync(CreatedStateId);

        await Assert.That(state).IsNotNull();
        await Assert.That(state!.Id).IsEqualTo(CreatedStateId);
        await Assert.That(state.Name).IsEqualTo(StateName);
        await Assert.That(state.Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(GetTicketState))]
    public async Task UpdateTicketState()
    {
        var client = await zammadStack.GetClientAsync();

        var state = await client.GetTicketStateAsync(CreatedStateId);
        await Assert.That(state).IsNotNull();
        state!.Note = "Updated note";

        var updated = await client.UpdateTicketStateAsync(CreatedStateId, state);

        await Assert.That(updated.Note).IsEqualTo("Updated note");
    }

    [Test]
    [DependsOn(nameof(UpdateTicketState))]
    public async Task DeleteTicketState()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.DeleteTicketStateAsync(CreatedStateId);

        await Assert.That(result).IsTrue();
    }
}
