using System.Linq;
using System.Threading.Tasks;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TagAdminTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();
    private static readonly string TagName = "AdminTag" + Id;
    private static TagId? CreatedTagId { get; set; }

    [Test]
    public async Task CreateTagAdmin()
    {
        var client = await zammadStack.GetClientAsync();
        await client.CreateTagAdminAsync(TagName);
    }

    [Test]
    [DependsOn(nameof(CreateTagAdmin))]
    public async Task ListTagsAdmin()
    {
        var client = await zammadStack.GetClientAsync();
        var results = await client.ListTagsAdminAsync();
        await Assert.That(results).HasAtLeast(1);
        await Assert.That(results).Contains(t => t.Name == TagName);
        CreatedTagId = results.FirstOrDefault(t => t.Name == TagName)?.Id;
        await Assert.That(CreatedTagId).IsNotNull();
    }

    [Test]
    [DependsOn(nameof(CreateTagAdmin))]
    public async Task SearchTags()
    {
        var client = await zammadStack.GetClientAsync();
        var results = await client.SearchTagsAsync(TagName);
        await Assert.That(results).HasAtLeast(1);
        await Assert.That(results).Contains(t => t.Value == TagName);
    }

    [Test]
    [DependsOn(nameof(CreateTagAdmin))]
    public async Task SearchTagsWithLimit()
    {
        var client = await zammadStack.GetClientAsync();
        var results = await client.SearchTagsAsync(TagName, 20);
        await Assert.That(results).HasAtLeast(1);
        await Assert.That(results).Contains(t => t.Value == TagName);
    }

    [Test]
    [DependsOn(nameof(ListTagsAdmin))]
    [DependsOn(nameof(SearchTags))]
    [DependsOn(nameof(SearchTagsWithLimit))]
    public async Task RenameTagAdmin()
    {
        await Assert.That(CreatedTagId).IsNotNull();
        var client = await zammadStack.GetClientAsync();
        await client.RenameTagAdminAsync(CreatedTagId.Value, "AdminTagRenamed" + Id);
    }

    [Test]
    [DependsOn(nameof(RenameTagAdmin))]
    public async Task DeleteTagAdmin()
    {
        await Assert.That(CreatedTagId).IsNotNull();
        var client = await zammadStack.GetClientAsync();
        await client.DeleteTagAdminAsync(CreatedTagId.Value);
    }
}
