using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class GroupTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string GroupName = "TestGroup" + RandomName;
    private static GroupId CreatedGroupId { get; set; } = GroupId.Empty;

    [Test]
    public async Task CreateGroup()
    {
        var client = await zammadStack.GetClientAsync();

        var group = await client.CreateGroupAsync(new Group { Name = GroupName, Active = true });

        await Assert.That(group).IsNotNull();
        await Assert.That(group.Id).IsNotEqualTo(GroupId.Empty);
        await Assert.That(group.Name).IsEqualTo(GroupName);
        await Assert.That(group.Active).IsTrue();

        CreatedGroupId = group.Id;
    }

    [Test]
    [DependsOn(nameof(CreateGroup))]
    public async Task ListGroups()
    {
        var client = await zammadStack.GetClientAsync();

        var groups = await client.ListGroupsAsync();

        await Assert.That(groups).HasAtLeast(1);
        await Assert.That(groups).Contains(g => g.Id == CreatedGroupId);
    }

    [Test]
    [DependsOn(nameof(CreateGroup))]
    public async Task ListGroups_Pagination()
    {
        var client = await zammadStack.GetClientAsync();

        var groups = await client.ListGroupsAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(groups).HasAtLeast(1);
        await Assert.That(groups).Contains(g => g.Id == CreatedGroupId);
    }

    [Test]
    [DependsOn(nameof(ListGroups))]
    [DependsOn(nameof(ListGroups_Pagination))]
    public async Task GetGroup()
    {
        var client = await zammadStack.GetClientAsync();

        var group = await client.GetGroupAsync(CreatedGroupId);

        await Assert.That(group).IsNotNull();
        await Assert.That(group!.Id).IsEqualTo(CreatedGroupId);
        await Assert.That(group.Name).IsEqualTo(GroupName);
        await Assert.That(group.NameLast).IsEqualTo(GroupName);
        await Assert.That(group.Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(CreateGroup))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchGroups(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var groups = await client.SearchGroupsAsync(
            new SearchQuery
            {
                Query = GroupName,
                Pagination = new Pagination { PerPage = 20 },
            }
        );

        await Assert.That(groups).HasSingleItem();
        await Assert.That(groups[0].Id).IsEqualTo(CreatedGroupId);
        await Assert.That(groups[0].Name).IsEqualTo(GroupName);
    }

    [Test]
    [DependsOn(nameof(SearchGroups))]
    public async Task SearchGroups_NotExpanded()
    {
        var client = await zammadStack.GetClientAsync();

        var groups = await client.SearchGroupsAsync(new SearchQuery { Query = GroupName }, expand: false);

        await Assert.That(groups).HasSingleItem();
        await Assert.That(groups[0].Id).IsEqualTo(CreatedGroupId);
    }

    [Test]
    [DependsOn(nameof(GetGroup))]
    [DependsOn(nameof(SearchGroups))]
    [DependsOn(nameof(SearchGroups_NotExpanded))]
    public async Task UpdateGroup()
    {
        var client = await zammadStack.GetClientAsync();

        var group = await client.GetGroupAsync(CreatedGroupId);
        await Assert.That(group).IsNotNull();
        group!.Note = "Updated note";

        var updated = await client.UpdateGroupAsync(CreatedGroupId, group);

        await Assert.That(updated.Note).IsEqualTo("Updated note");
    }

    [Test]
    [DependsOn(nameof(UpdateGroup))]
    public async Task DeleteGroup()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteGroupAsync(CreatedGroupId);
        await Assert.That(await client.GetGroupAsync(CreatedGroupId)).IsNull();
    }
}
