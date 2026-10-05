using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Setup;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class MonitoringTests(ZammadStackFixture zammadStack)
{
    [Test]
    public async Task HealthCheck()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.HealthCheckAsync();

        await Assert.That(result.Healthy).IsTrue();
        await Assert.That(result.Message).IsEqualTo("success");
        await Assert.That(result.Issues).IsEmpty();
        await Assert.That(result.Actions).IsEmpty();
        await Assert.That(result.Token).IsNotNullOrEmpty();
    }

    [Test]
    public async Task GetMonitoringStatus()
    {
        var client = await zammadStack.GetClientAsync();

        var status = await client.GetMonitoringStatusAsync();

        // The auto wizard creates the admin and agent1, and Zammad seeds a welcome ticket
        await Assert.That(status.Agents).IsGreaterThanOrEqualTo(2);
        await Assert.That(status.Counts).ContainsKey("users");
        await Assert.That(status.Counts["tickets"]).IsGreaterThanOrEqualTo(1);
        await Assert.That(status.LastCreatedAt["tickets"]).IsNotNull();
    }

    [Test]
    public async Task AmountCheck_WithoutThresholds()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.AmountCheckAsync(new AmountCheckQuery { Period = TimeSpan.FromDays(1) });

        await Assert.That(result.State).IsNull();
        await Assert.That(result.Message).IsNull();
        await Assert.That(result.Count).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task AmountCheck_Ok()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.AmountCheckAsync(
            new AmountCheckQuery { Period = TimeSpan.FromHours(1), MaxWarning = 1_000_000 }
        );

        await Assert.That(result.State).IsEqualTo("ok");
        await Assert.That(result.Message).IsNull();
    }

    [Test]
    public async Task AmountCheck_BelowMinimum()
    {
        var client = await zammadStack.GetClientAsync();

        // The minimum check fails if count <= the threshold
        var result = await client.AmountCheckAsync(
            new AmountCheckQuery { Period = TimeSpan.FromMinutes(5), MinCritical = 1_000_000 }
        );

        await Assert.That(result.State).IsEqualTo("critical");
        await Assert.That(result.Message).StartsWith("The minimum of 1000000 was undercut");
        await Assert.That(result.Message).EndsWith("in the last 5m");
    }

    [Test]
    public async Task RestartFailedJobs()
    {
        var client = await zammadStack.GetClientAsync();

        await client.RestartFailedJobsAsync();
    }
}
