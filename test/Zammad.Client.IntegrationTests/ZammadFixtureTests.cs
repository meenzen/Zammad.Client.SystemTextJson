using System;
using System.Threading.Tasks;
using Xunit;
using Zammad.Client.IntegrationTests.Setup;

namespace Zammad.Client.IntegrationTests;

public class ZammadFixtureTests(ZammadStackFixture zammadStack)
{
    [Fact(Skip = "Manual test for debugging")]
    public async Task Stack()
    {
        var url = await zammadStack.GetPublicUriAsync();
        TestContext.Current.TestOutputHelper!.WriteLine("Zammad URL: {0}", url);
        await Task.Delay(TimeSpan.FromMinutes(15), TestContext.Current.CancellationToken);
    }
}
