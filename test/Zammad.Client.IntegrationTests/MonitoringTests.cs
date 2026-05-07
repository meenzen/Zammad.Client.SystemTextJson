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
}
