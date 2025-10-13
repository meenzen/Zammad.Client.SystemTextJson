using System.Threading.Tasks;
using Xunit;
using Zammad.Client.IntegrationTests.Setup;

namespace Zammad.Client.IntegrationTests;

[TestCaseOrderer(typeof(TestOrderer))]
public class TagClientTest(ZammadStackFixture zammadStack)
{
    [Fact, Order(TestOrder.TagGetTagList)]
    public async Task Tag_TagGetTagList_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var tagList = await client.GetTagListAsync("Ticket", 1);
        Assert.NotNull(tagList);
    }
}
