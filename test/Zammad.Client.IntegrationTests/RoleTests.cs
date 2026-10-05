using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

/// <remarks>
/// Roles can't be deleted, so this creates a role with a random name and deactivates it at the end. The seeded roles
/// are global data that other tests rely on and are never modified.
/// </remarks>
[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class RoleTests(ZammadStackFixture zammadStack)
{
    private static readonly string Name = "Test Role " + TestSetup.RandomString();
    private static RoleId CreatedRoleId { get; set; } = RoleId.Empty;

    [Test]
    public async Task ListRoles_ContainsSeededRoles()
    {
        var client = await zammadStack.GetClientAsync();

        var roles = await client.ListRolesAsync();

        await Assert.That(roles).Contains(r => r.Name == "Admin");
        await Assert.That(roles).Contains(r => r.Name == "Agent");
        await Assert.That(roles).Contains(r => r.Name == "Customer");
    }

    [Test]
    public async Task CreateRole()
    {
        var client = await zammadStack.GetClientAsync();

        var role = await client.CreateRoleAsync(
            new Role
            {
                Name = Name,
                Note = "Created by integration test",
                Active = true,
                Permissions = ["ticket.agent"],
                GroupIds = new() { [new GroupId(1)] = ["read"] },
            }
        );

        await Assert.That(role.Id).IsNotEqualTo(RoleId.Empty);
        await Assert.That(role.Name).IsEqualTo(Name);
        await Assert.That(role.Active).IsTrue();
        await Assert.That(role.PermissionIds).HasSingleItem();
        await Assert.That(role.GroupIds).IsNotNull();
        await Assert.That(role.GroupIds![new GroupId(1)]).IsEquivalentTo(["read"]);

        CreatedRoleId = role.Id;
    }

    [Test]
    [DependsOn(nameof(CreateRole))]
    public async Task ListRoles()
    {
        var client = await zammadStack.GetClientAsync();

        var roles = await client.ListRolesAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(roles).Contains(r => r.Id == CreatedRoleId);
    }

    [Test]
    [DependsOn(nameof(CreateRole))]
    public async Task GetRole()
    {
        var client = await zammadStack.GetClientAsync();

        var role = await client.GetRoleAsync(CreatedRoleId);

        await Assert.That(role).IsNotNull();
        await Assert.That(role!.Id).IsEqualTo(CreatedRoleId);
        await Assert.That(role.Name).IsEqualTo(Name);
        await Assert.That(role.Note).IsEqualTo("Created by integration test");
    }

    [Test]
    [DependsOn(nameof(CreateRole))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchRoles()
    {
        await Task.Delay(TestSetup.IndexerDelay);
        var client = await zammadStack.GetClientAsync();

        var roles = await client.SearchRolesAsync(new SearchQuery { Query = Name });

        var role = roles.SingleOrDefault(r => r.Id == CreatedRoleId);
        await Assert.That(role).IsNotNull();
        // expanded by default
        await Assert.That(role!.Permissions).IsEquivalentTo(["ticket.agent"]);
        // group 1 is the seeded "Users" group
        await Assert.That(role.Groups).ContainsKey("Users");
    }

    [Test]
    [DependsOn(nameof(GetRole))]
    [DependsOn(nameof(ListRoles))]
    [DependsOn(nameof(SearchRoles))]
    public async Task UpdateRole()
    {
        var client = await zammadStack.GetClientAsync();

        var updated = await client.UpdateRoleAsync(
            CreatedRoleId,
            new Role
            {
                Name = Name,
                Note = "Updated by integration test",
                Permissions = ["ticket.agent", "report"],
                GroupIds = new() { [new GroupId(1)] = ["read", "create"] },
            }
        );

        await Assert.That(updated.Id).IsEqualTo(CreatedRoleId);
        await Assert.That(updated.Note).IsEqualTo("Updated by integration test");
        await Assert.That(updated.PermissionIds).Count().IsEqualTo(2);
        await Assert.That(updated.GroupIds![new GroupId(1)]).IsEquivalentTo(["create", "read"]);
    }

    [Test]
    [DependsOn(nameof(UpdateRole))]
    public async Task DeactivateRole()
    {
        var client = await zammadStack.GetClientAsync();

        var updated = await client.UpdateRoleAsync(CreatedRoleId, new Role { Active = false });

        await Assert.That(updated.Active).IsFalse();
        await Assert.That(updated.PermissionIds).Count().IsEqualTo(2);
        // Zammad keeps the group access of inactive roles, but doesn't return it
        await Assert.That(updated.GroupIds).IsEmpty();
    }
}
