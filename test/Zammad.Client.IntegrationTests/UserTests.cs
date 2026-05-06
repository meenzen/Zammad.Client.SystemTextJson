using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class UserTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static UserId HomerSimpsonId { get; set; } = UserId.Empty;
    private static UserId MargeSimpsonId { get; set; } = UserId.Empty;
    private static UserId BartSimpsonId { get; set; } = UserId.Empty;

    [Test]
    public async Task GetUserMe()
    {
        var client = await zammadStack.GetClientAsync();

        var user = await client.GetUserMeAsync();

        await Assert.That(user).IsNotNull();
        await Assert.That(user.Email).IsEqualTo("admin@example.org");
    }

    [Test]
    public async Task CreateUser()
    {
        var client = await zammadStack.GetClientAsync();

        var user1 = await client.CreateUserAsync(
            new User
            {
                FirstName = "Homer",
                LastName = "Simpson" + RandomName,
                Email = $"homer.simpson.{RandomName}@springfield.com",
                Login = $"homer.{RandomName}",
                Active = true,
            }
        );

        var user2 = await client.CreateUserAsync(
            new User
            {
                FirstName = "Marge",
                LastName = "Simpson" + RandomName,
                Email = $"marge.simpson.{RandomName}@springfield.com",
                Login = $"marge.{RandomName}",
                Active = true,
            }
        );

        var user3 = await client.CreateUserAsync(
            new User
            {
                FirstName = "Bart",
                LastName = "Simpson" + RandomName,
                Email = $"bart.simpson.{RandomName}@springfield.com",
                Login = $"bart.{RandomName}",
                Active = true,
            }
        );

        await Assert.That(user1).IsNotNull();
        await Assert.That(user2).IsNotNull();
        await Assert.That(user3).IsNotNull();

        HomerSimpsonId = user1.Id;
        MargeSimpsonId = user2.Id;
        BartSimpsonId = user3.Id;
    }

    [Test]
    [DependsOn(nameof(CreateUser))]
    public async Task ListUsers()
    {
        var client = await zammadStack.GetClientAsync();

        var userList = await client.ListUsersAsync();

        await Assert.That(userList).HasAtLeast(3);
        await Assert.That(userList).Contains(u => u.Id == HomerSimpsonId);
        await Assert.That(userList).Contains(u => u.Id == MargeSimpsonId);
        await Assert.That(userList).Contains(u => u.Id == BartSimpsonId);
    }

    [Test]
    [DependsOn(nameof(CreateUser))]
    public async Task ListUsers_Pagination()
    {
        var client = await zammadStack.GetClientAsync();

        var userList = await client.ListUsersAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(userList).HasAtLeast(3);
        await Assert.That(userList).Contains(u => u.Id == HomerSimpsonId);
        await Assert.That(userList).Contains(u => u.Id == MargeSimpsonId);
        await Assert.That(userList).Contains(u => u.Id == BartSimpsonId);
    }

    [Test]
    [DependsOn(nameof(CreateUser))]
    [Obsolete("Testing legacy pagination.")]
    public async Task ListUsers_Pagination_Legacy()
    {
        var client = await zammadStack.GetClientAsync();

        var userList = await client.ListUsersAsync(1, 100);

        await Assert.That(userList).HasAtLeast(3);
        await Assert.That(userList).Contains(u => u.Id == HomerSimpsonId);
        await Assert.That(userList).Contains(u => u.Id == MargeSimpsonId);
        await Assert.That(userList).Contains(u => u.Id == BartSimpsonId);
    }

    [Test]
    [DependsOn(nameof(ListUsers))]
    [DependsOn(nameof(ListUsers_Pagination))]
    [DependsOn(nameof(ListUsers_Pagination_Legacy))]
    public async Task GetUser()
    {
        var client = await zammadStack.GetClientAsync();

        var user = await client.GetUserAsync(HomerSimpsonId);

        await Assert.That(user).IsNotNull();
        await Assert.That(user!.Id).IsEqualTo(HomerSimpsonId);
        await Assert.That(user.FirstName).IsEqualTo("Homer");
        await Assert.That(user.LastName).StartsWith("Simpson");
        await Assert.That(user.Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(GetUser))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchUsers(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var userSearch = await client.SearchUsersAsync(
            new SearchQuery
            {
                Query = $"homer.simpson.{RandomName}",
                Pagination = new Pagination { PerPage = 20 },
            }
        );

        await Assert.That(userSearch).HasSingleItem();
        await Assert.That(userSearch[0].Id).IsEqualTo(HomerSimpsonId);
    }

    [Test]
    [DependsOn(nameof(GetUser))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    [Obsolete("Testing legacy search.")]
    public async Task SearchUsers_Legacy(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var userSearch = await client.SearchUsersAsync($"homer.simpson.{RandomName}", 20);

        await Assert.That(userSearch).HasSingleItem();
        await Assert.That(userSearch[0].Id).IsEqualTo(HomerSimpsonId);
    }

    [Test]
    [DependsOn(nameof(GetUser))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    [Obsolete("Testing legacy search with sort.")]
    public async Task SearchUsers_LegacyWithSort(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var userSearch = await client.SearchUsersAsync($"homer.simpson.{RandomName}", 20, "id", "asc");

        await Assert.That(userSearch).HasSingleItem();
        await Assert.That(userSearch[0].Id).IsEqualTo(HomerSimpsonId);
    }

    [Test]
    [DependsOn(nameof(SearchUsers))]
    [DependsOn(nameof(SearchUsers_Legacy))]
    [DependsOn(nameof(SearchUsers_LegacyWithSort))]
    public async Task UpdateUser()
    {
        var client = await zammadStack.GetClientAsync();

        var user = await client.GetUserAsync(HomerSimpsonId);
        await Assert.That(user).IsNotNull();
        user!.Phone = "555-HOMER";

        var updated = await client.UpdateUserAsync(HomerSimpsonId, user);

        await Assert.That(updated.Phone).IsEqualTo("555-HOMER");
    }

    [Test]
    [DependsOn(nameof(UpdateUser))]
    public async Task DeleteUser()
    {
        var client = await zammadStack.GetClientAsync();

        var result1 = await client.DeleteUserAsync(HomerSimpsonId);
        var result2 = await client.DeleteUserAsync(MargeSimpsonId);
        var result3 = await client.DeleteUserAsync(BartSimpsonId);

        await Assert.That(result1).IsTrue();
        await Assert.That(result2).IsTrue();
        await Assert.That(result3).IsTrue();
    }
}
