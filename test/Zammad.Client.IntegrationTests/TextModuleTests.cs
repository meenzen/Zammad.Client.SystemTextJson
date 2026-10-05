using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TextModuleTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string TextModuleName = "TestTextModule" + RandomName;

    // "Users", created by the auto wizard
    private static readonly GroupId UsersGroupId = new(1);
    private static TextModuleId CreatedTextModuleId { get; set; } = TextModuleId.Empty;

    [Test]
    public async Task CreateTextModule()
    {
        var client = await zammadStack.GetClientAsync();

        var textModule = await client.CreateTextModuleAsync(
            new TextModule
            {
                Name = TextModuleName,
                Keywords = "test" + RandomName,
                Content = "Hello\nWorld",
                Note = "Test note",
                Active = true,
                GroupIds = [UsersGroupId],
            }
        );

        await Assert.That(textModule.Id).IsNotEqualTo(TextModuleId.Empty);
        await Assert.That(textModule.Name).IsEqualTo(TextModuleName);
        await Assert.That(textModule.Keywords).IsEqualTo("test" + RandomName);
        // plain text is converted to HTML
        await Assert.That(textModule.Content).IsEqualTo("Hello<br>World");
        await Assert.That(textModule.GroupIds).IsEquivalentTo([UsersGroupId]);

        CreatedTextModuleId = textModule.Id;
    }

    [Test]
    [DependsOn(nameof(CreateTextModule))]
    public async Task ListTextModules()
    {
        var client = await zammadStack.GetClientAsync();

        var textModules = await client.ListTextModulesAsync(new Pagination { Page = 1, PerPage = 500 });

        await Assert.That(textModules).Contains(t => t.Id == CreatedTextModuleId);
    }

    [Test]
    [DependsOn(nameof(CreateTextModule))]
    public async Task GetTextModule()
    {
        var client = await zammadStack.GetClientAsync();

        var textModule = await client.GetTextModuleAsync(CreatedTextModuleId);

        await Assert.That(textModule).IsNotNull();
        await Assert.That(textModule!.Name).IsEqualTo(TextModuleName);
        await Assert.That(textModule.Note).IsEqualTo("Test note");
        await Assert.That(textModule.Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(CreateTextModule))]
    public async Task GetTextModule_AsAgent()
    {
        var client = await zammadStack.GetClientOnBehalfOfAsync("agent1@example.org");

        await Assert.That(await client.GetTextModuleAsync(CreatedTextModuleId)).IsNotNull();
    }

    [Test]
    [DependsOn(nameof(CreateTextModule))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchTextModules(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var textModules = await client.SearchTextModulesAsync(new SearchQuery { Query = TextModuleName });

        await Assert.That(textModules).HasSingleItem();
        await Assert.That(textModules[0].Id).IsEqualTo(CreatedTextModuleId);
        await Assert.That(textModules[0].Groups).IsEquivalentTo(["Users"]);
    }

    [Test]
    [DependsOn(nameof(ListTextModules))]
    [DependsOn(nameof(GetTextModule))]
    [DependsOn(nameof(GetTextModule_AsAgent))]
    [DependsOn(nameof(SearchTextModules))]
    public async Task UpdateTextModule()
    {
        var client = await zammadStack.GetClientAsync();

        var textModule = await client.GetTextModuleAsync(CreatedTextModuleId);
        await Assert.That(textModule).IsNotNull();
        textModule!.Content = "<p>Updated content</p>";
        textModule.GroupIds = [];

        var updated = await client.UpdateTextModuleAsync(CreatedTextModuleId, textModule);

        await Assert.That(updated.Content).IsEqualTo("<p>Updated content</p>");
        await Assert.That(updated.GroupIds).IsEmpty();
    }

    [Test]
    [DependsOn(nameof(UpdateTextModule))]
    public async Task DeleteTextModule()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTextModuleAsync(CreatedTextModuleId);
        await Assert.That(await client.GetTextModuleAsync(CreatedTextModuleId)).IsNull();
    }
}
