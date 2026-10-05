using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client.Tests.Resources;

public class OverviewPrioRequestTests
{
    [Test]
    public async Task SerializesAsIdPrioPairs()
    {
        var request = OverviewPrioRequest.Create(
            new Dictionary<OverviewId, int> { [new OverviewId(3)] = 1, [new OverviewId(7)] = 2 }
        );

        var json = JsonSerializer.Serialize(request, Serialization.Options);

        await Assert.That(json).IsEqualTo("""{"prios":[[3,1],[7,2]]}""");
    }
}
