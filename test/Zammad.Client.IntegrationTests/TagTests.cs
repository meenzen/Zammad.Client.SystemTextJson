using System.Threading.Tasks;
using Xunit;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[TestCaseOrderer(typeof(TestOrderer))]
public class TagTests(ZammadStackFixture zammadStack)
{
    [Fact, Order(TestOrder.TagGetTagList)]
    public async Task Tag_TagGetTagList_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var tagList = await client.ListTagsAsync("Ticket", new ObjectId(1));
        Assert.NotNull(tagList);
    }
}
