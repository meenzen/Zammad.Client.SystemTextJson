using System.Threading.Tasks;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TagTests(ZammadStackFixture zammadStack)
{
    [Test]
    [DependsOn<TicketTests>(nameof(TicketTests.Ticket_Create_Test))]
    public async Task Tag_TagGetTagList_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var tagList = await client.ListTagsAsync("Ticket", new ObjectId(1));
        await Assert.That(tagList).IsNotNull();
    }
}
