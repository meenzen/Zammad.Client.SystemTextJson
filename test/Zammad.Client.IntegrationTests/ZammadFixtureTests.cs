using System;
using System.Threading;
using System.Threading.Tasks;
using Zammad.Client.IntegrationTests.Setup;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class ZammadFixtureTests(ZammadStackFixture zammadStack)
{
    [Test]
    [Skip("Manual test for debugging")]
    public async Task Stack(CancellationToken cancellationToken)
    {
        var url = await zammadStack.GetPublicUriAsync();
        TestContext.Current!.Output.WriteLine($"Zammad URL: {url}");
        await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
    }
}
