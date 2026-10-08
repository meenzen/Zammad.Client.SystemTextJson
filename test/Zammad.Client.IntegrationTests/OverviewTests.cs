using System.Net;
using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class OverviewTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string OverviewName = "TestOverview" + RandomName;

    // "Agent", seeded by Zammad
    private static readonly RoleId AgentRoleId = new(2);
    private static OverviewId CreatedOverviewId { get; set; } = OverviewId.Empty;
    private static OverviewId SecondOverviewId { get; set; } = OverviewId.Empty;

    private static Overview NewOverview(string name) =>
        new()
        {
            Name = name,
            RoleIds = [AgentRoleId],
            Condition = JsonSerializer.SerializeToElement(
                new Dictionary<string, object>
                {
                    ["ticket.state_id"] = new { @operator = "is", value = new[] { "1", "2", "3" } },
                    ["ticket.owner_id"] = new { @operator = "is", pre_condition = "current_user.id" },
                }
            ),
            Order = new OverviewOrder { By = "created_at", Direction = "DESC" },
            GroupBy = "priority",
            GroupDirection = "ASC",
            View = new OverviewView { Columns = ["number", "title", "customer", "state", "created_at"] },
            Active = true,
        };

    [Test]
    public async Task CreateOverview()
    {
        var client = await zammadStack.GetClientAsync();
        var me = await client.GetUserMeAsync();
        var overview = NewOverview(OverviewName);
        overview.UserIds = [me.Id];

        var created = await client.CreateOverviewAsync(overview);

        await Assert.That(created.Id).IsNotEqualTo(OverviewId.Empty);
        await Assert.That(created.Name).IsEqualTo(OverviewName);
        // generated from the name
        await Assert.That(created.Link).IsEqualTo(OverviewName.ToLowerInvariant());
        await Assert.That(created.Prio).IsNotNull();
        await Assert.That(created.RoleIds).IsEquivalentTo([AgentRoleId]);
        await Assert.That(created.UserIds).IsEquivalentTo([me.Id]);
        await Assert.That(created.Order!.Direction).IsEqualTo("DESC");
        await Assert.That(created.View!.Columns).IsEquivalentTo(["number", "title", "customer", "state", "created_at"]);
        await Assert
            .That(created.Condition!.Value.GetProperty("ticket.owner_id").GetProperty("pre_condition").GetString())
            .IsEqualTo("current_user.id");

        CreatedOverviewId = created.Id;
    }

    [Test]
    [DependsOn(nameof(CreateOverview))]
    public async Task CreateOverview_SameNameGetsUniqueLink()
    {
        var client = await zammadStack.GetClientAsync();

        var second = await client.CreateOverviewAsync(NewOverview(OverviewName));

        await Assert.That(second.Link).IsEqualTo(OverviewName.ToLowerInvariant() + "_1");

        SecondOverviewId = second.Id;
    }

    [Test]
    public async Task CreateOverview_ThrowsWithoutRoles()
    {
        var client = await zammadStack.GetClientAsync();
        var overview = NewOverview("TestOverviewNoRoles" + RandomName);
        overview.RoleIds = [];

        var exception = await Assert.ThrowsAsync<ZammadException>(() => client.CreateOverviewAsync(overview));

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task CreateOverview_ThrowsWithInvalidDirection()
    {
        var client = await zammadStack.GetClientAsync();
        var overview = NewOverview("TestOverviewInvalidDirection" + RandomName);
        overview.Order = new OverviewOrder { By = "created_at", Direction = "SIDEWAYS" };

        var exception = await Assert.ThrowsAsync<ZammadException>(() => client.CreateOverviewAsync(overview));

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert
            .That(exception.Error)
            .IsEqualTo("Invalid order direction 'SIDEWAYS', only ASC or DESC are allowed.");
    }

    [Test]
    [DependsOn(nameof(CreateOverview))]
    public async Task ListOverviews()
    {
        var client = await zammadStack.GetClientAsync();

        var overviews = await client.ListOverviewsAsync(new Pagination { Page = 1, PerPage = 500 });

        await Assert.That(overviews).Contains(o => o.Id == CreatedOverviewId);
    }

    [Test]
    [DependsOn(nameof(CreateOverview))]
    public async Task GetOverview()
    {
        var client = await zammadStack.GetClientAsync();

        var overview = await client.GetOverviewAsync(CreatedOverviewId);

        await Assert.That(overview).IsNotNull();
        await Assert.That(overview!.Name).IsEqualTo(OverviewName);
        await Assert.That(overview.GroupBy).IsEqualTo("priority");
        await Assert.That(overview.GroupDirection).IsEqualTo("ASC");
        await Assert.That(overview.Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(CreateOverview))]
    public async Task GetOverview_ThrowsForAgent()
    {
        var client = await zammadStack.GetClientOnBehalfOfAsync("agent1@example.org");

        var exception = await Assert.ThrowsAsync<ZammadException>(() => client.GetOverviewAsync(CreatedOverviewId));

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    [DependsOn(nameof(CreateOverview))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchOverviews(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var overviews = await client.SearchOverviewsAsync(new SearchQuery { Query = OverviewName });

        await Assert.That(overviews).Contains(o => o.Id == CreatedOverviewId);
        await Assert.That(overviews.First(o => o.Id == CreatedOverviewId).Roles).IsEquivalentTo(["Agent"]);
    }

    [Test]
    [DependsOn(nameof(CreateOverview_SameNameGetsUniqueLink))]
    public async Task SetOverviewPriorities()
    {
        var client = await zammadStack.GetClientAsync();
        var first = await client.GetOverviewAsync(CreatedOverviewId);
        var second = await client.GetOverviewAsync(SecondOverviewId);
        await Assert.That(first).IsNotNull();
        await Assert.That(second).IsNotNull();

        // swap the two overviews, the others aren't touched
        await client.SetOverviewPrioritiesAsync(
            new Dictionary<OverviewId, int>
            {
                [CreatedOverviewId] = second!.Prio!.Value,
                [SecondOverviewId] = first!.Prio!.Value,
            }
        );

        await Assert.That((await client.GetOverviewAsync(CreatedOverviewId))!.Prio).IsEqualTo(second.Prio);
        await Assert.That((await client.GetOverviewAsync(SecondOverviewId))!.Prio).IsEqualTo(first.Prio);
    }

    [Test]
    [DependsOn(nameof(ListOverviews))]
    [DependsOn(nameof(GetOverview))]
    [DependsOn(nameof(GetOverview_ThrowsForAgent))]
    [DependsOn(nameof(SearchOverviews))]
    [DependsOn(nameof(SetOverviewPriorities))]
    public async Task UpdateOverview()
    {
        var client = await zammadStack.GetClientAsync();

        // keep the prio: changing it renumbers all other overviews
        var overview = await client.GetOverviewAsync(CreatedOverviewId);
        await Assert.That(overview).IsNotNull();
        overview!.Order = new OverviewOrder { By = "updated_at", Direction = "ASC" };
        overview.Active = false;

        var updated = await client.UpdateOverviewAsync(CreatedOverviewId, overview);

        await Assert.That(updated.Order!.By).IsEqualTo("updated_at");
        await Assert.That(updated.Active).IsFalse();
        await Assert.That(updated.Prio).IsEqualTo(overview.Prio);
    }

    [Test]
    [DependsOn(nameof(UpdateOverview))]
    public async Task DeleteOverview()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteOverviewAsync(CreatedOverviewId);
        await client.DeleteOverviewAsync(SecondOverviewId);
        await Assert.That(await client.GetOverviewAsync(CreatedOverviewId)).IsNull();
        await Assert.That(await client.GetOverviewAsync(SecondOverviewId)).IsNull();
    }
}
