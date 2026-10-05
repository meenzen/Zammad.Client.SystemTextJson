using System.Net;
using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class MacroTests(ZammadStackFixture zammadStack)
{
    private const string AgentLogin = "agent1@example.org";
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string MacroName = "TestMacro" + RandomName;
    private static readonly string Tag = "macrotag" + RandomName;

    // "Users", created by the auto wizard
    private static readonly GroupId UsersGroupId = new(1);
    private static MacroId CreatedMacroId { get; set; } = MacroId.Empty;

    [Test]
    public async Task CreateMacro()
    {
        var client = await zammadStack.GetClientAsync();

        var macro = await client.CreateMacroAsync(
            new Macro
            {
                Name = MacroName,
                Perform = JsonSerializer.SerializeToElement(
                    new Dictionary<string, object>
                    {
                        ["ticket.priority_id"] = new { value = "3" },
                        ["ticket.tags"] = new { @operator = "add", value = Tag },
                    }
                ),
                UxFlowNextUp = "next_task",
                Note = "Test note",
                Active = true,
                GroupIds = [UsersGroupId],
            }
        );

        await Assert.That(macro.Id).IsNotEqualTo(MacroId.Empty);
        await Assert.That(macro.Name).IsEqualTo(MacroName);
        await Assert.That(macro.UxFlowNextUp).IsEqualTo("next_task");
        await Assert.That(macro.GroupIds).IsEquivalentTo([UsersGroupId]);
        await Assert
            .That(macro.Perform!.Value.GetProperty("ticket.tags").GetProperty("value").GetString())
            .IsEqualTo(Tag);

        CreatedMacroId = macro.Id;
    }

    [Test]
    [DependsOn(nameof(CreateMacro))]
    public async Task CreateMacro_ThrowsForDuplicateName()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateMacroAsync(
                new Macro
                {
                    Name = MacroName,
                    Perform = JsonSerializer.SerializeToElement(
                        new Dictionary<string, object> { ["ticket.priority_id"] = new { value = "3" } }
                    ),
                }
            )
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [DependsOn(nameof(CreateMacro))]
    public async Task ListMacros()
    {
        var client = await zammadStack.GetClientAsync();

        var macros = await client.ListMacrosAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(macros).Contains(m => m.Id == CreatedMacroId);
    }

    [Test]
    [DependsOn(nameof(CreateMacro))]
    public async Task GetMacro()
    {
        var client = await zammadStack.GetClientAsync();

        var macro = await client.GetMacroAsync(CreatedMacroId);

        await Assert.That(macro).IsNotNull();
        await Assert.That(macro!.Name).IsEqualTo(MacroName);
        await Assert.That(macro.Note).IsEqualTo("Test note");
        await Assert.That(macro.Active).IsTrue();
        await Assert
            .That(macro.Perform!.Value.GetProperty("ticket.priority_id").GetProperty("value").GetString())
            .IsEqualTo("3");
    }

    [Test]
    [DependsOn(nameof(CreateMacro))]
    public async Task GetMacro_AsAgent()
    {
        var client = await zammadStack.GetClientOnBehalfOfAsync(AgentLogin);

        var macro = await client.GetMacroAsync(CreatedMacroId);
        var macros = await client.ListMacrosAsync();

        await Assert.That(macro).IsNotNull();
        await Assert.That(macros).Contains(m => m.Id == CreatedMacroId);
    }

    [Test]
    [DependsOn(nameof(CreateMacro))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchMacros(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var macros = await client.SearchMacrosAsync(new SearchQuery { Query = MacroName });

        await Assert.That(macros).HasSingleItem();
        await Assert.That(macros[0].Id).IsEqualTo(CreatedMacroId);
        await Assert.That(macros[0].Groups).IsEquivalentTo(["Users"]);
    }

    [Test]
    [DependsOn(nameof(SearchMacros))]
    public async Task SearchMacros_NotExpanded()
    {
        var client = await zammadStack.GetClientAsync();

        var macros = await client.SearchMacrosAsync(new SearchQuery { Query = MacroName }, expand: false);

        await Assert.That(macros).HasSingleItem();
        await Assert.That(macros[0].Id).IsEqualTo(CreatedMacroId);
        await Assert.That(macros[0].Groups).IsNull();
    }

    [Test]
    [DependsOn(nameof(CreateMacro))]
    public async Task SearchMacros_ThrowsForAgent()
    {
        var client = await zammadStack.GetClientOnBehalfOfAsync(AgentLogin);

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.SearchMacrosAsync(new SearchQuery { Query = MacroName })
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    [DependsOn(nameof(CreateMacro_ThrowsForDuplicateName))]
    [DependsOn(nameof(ListMacros))]
    [DependsOn(nameof(GetMacro))]
    [DependsOn(nameof(GetMacro_AsAgent))]
    [DependsOn(nameof(SearchMacros_NotExpanded))]
    [DependsOn(nameof(SearchMacros_ThrowsForAgent))]
    public async Task UpdateMacro()
    {
        var client = await zammadStack.GetClientAsync();

        var macro = await client.GetMacroAsync(CreatedMacroId);
        await Assert.That(macro).IsNotNull();
        macro!.Note = "Updated note";
        macro.Active = false;

        var updated = await client.UpdateMacroAsync(CreatedMacroId, macro);

        await Assert.That(updated.Note).IsEqualTo("Updated note");
        await Assert.That(updated.Active).IsFalse();
    }

    [Test]
    [DependsOn(nameof(UpdateMacro))]
    public async Task GetMacro_AsAgent_ReturnsNullForInactiveMacro()
    {
        var client = await zammadStack.GetClientOnBehalfOfAsync(AgentLogin);

        await Assert.That(await client.GetMacroAsync(CreatedMacroId)).IsNull();
    }

    [Test]
    [DependsOn(nameof(GetMacro_AsAgent_ReturnsNullForInactiveMacro))]
    public async Task DeleteMacro()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteMacroAsync(CreatedMacroId);
        await Assert.That(await client.GetMacroAsync(CreatedMacroId)).IsNull();
    }
}
