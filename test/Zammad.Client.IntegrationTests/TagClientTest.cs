using System.Threading.Tasks;
using Xunit;

namespace Zammad.Client.IntegrationTests;

[TestCaseOrderer(typeof(TestOrderer))]
public class TagClientTest
{
    [Fact, Order(TestOrder.TagGetTagList)]
    public async Task Tag_TagGetTagList_Test()
    {
        var client = TestHelper.Client;

        var tagList = await client.GetTagListAsync("Ticket", 1);
        Assert.NotNull(tagList);
    }
}
