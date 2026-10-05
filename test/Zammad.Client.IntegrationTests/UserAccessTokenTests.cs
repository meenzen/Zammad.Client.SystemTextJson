using System.Net;
using Microsoft.Extensions.Options;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class UserAccessTokenTests(ZammadStackFixture zammadStack)
{
    private static readonly string Name = "Test Token " + TestSetup.RandomString();
    private static readonly DateTime ExpiresAt = DateTime.UtcNow.Date.AddDays(30);
    private static string Token { get; set; } = string.Empty;
    private static UserAccessTokenId CreatedTokenId { get; set; } = UserAccessTokenId.Empty;

    private async Task<IZammadClient> GetTokenClientAsync() =>
        new ZammadClient(
            new HttpClient(),
            Options.Create(new ZammadOptions { BaseUrl = await zammadStack.GetPublicUriAsync(), Token = Token })
        );

    [Test]
    public async Task ListUserAccessTokens_ReturnsGrantablePermissions()
    {
        var client = await zammadStack.GetClientAsync();

        var list = await client.ListUserAccessTokensAsync();

        await Assert.That(list.Permissions).Contains(p => p.Name == "ticket.agent");
        await Assert.That(list.Permissions).Contains(p => p.Name == "admin.role");
    }

    [Test]
    public async Task CreateUserAccessToken()
    {
        var client = await zammadStack.GetClientAsync();

        var token = await client.CreateUserAccessTokenAsync(Name, ["ticket.agent"], ExpiresAt);

        await Assert.That(token).IsNotNullOrWhiteSpace();

        Token = token;
    }

    [Test]
    [DependsOn(nameof(CreateUserAccessToken))]
    public async Task ListUserAccessTokens()
    {
        var client = await zammadStack.GetClientAsync();
        var me = await client.GetUserMeAsync();

        var list = await client.ListUserAccessTokensAsync();

        var token = list.Tokens.SingleOrDefault(t => t.Name == Name);
        await Assert.That(token).IsNotNull();
        await Assert.That(token!.Id).IsNotEqualTo(UserAccessTokenId.Empty);
        await Assert.That(token.UserId).IsEqualTo(me.Id);
        await Assert.That(token.Action).IsEqualTo("api");
        await Assert.That(token.Preferences?.Permission).IsEquivalentTo(["ticket.agent"]);
        await Assert.That(token.ExpiresAt).IsNotNull();
        await Assert.That(token.ExpiresAt!.Value.UtcDateTime).IsGreaterThan(ExpiresAt.AddDays(-2));
        await Assert.That(token.ExpiresAt!.Value.UtcDateTime).IsLessThan(ExpiresAt.AddDays(2));

        CreatedTokenId = token.Id;
    }

    [Test]
    [DependsOn(nameof(CreateUserAccessToken))]
    public async Task AuthenticateWithToken()
    {
        var admin = await zammadStack.GetClientAsync();
        var client = await GetTokenClientAsync();

        var me = await client.GetUserMeAsync();
        var priorities = await client.ListTicketPrioritiesAsync();

        await Assert.That(me.Id).IsEqualTo((await admin.GetUserMeAsync()).Id);
        await Assert.That(priorities).IsNotEmpty();
    }

    [Test]
    [DependsOn(nameof(CreateUserAccessToken))]
    public async Task AuthenticateWithToken_LimitedToTokenPermissions()
    {
        var client = await GetTokenClientAsync();

        // The admin may create roles, but the token only has ticket.agent
        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateRoleAsync(new Role { Name = "Forbidden Role " + TestSetup.RandomString() })
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    [DependsOn(nameof(ListUserAccessTokens))]
    [DependsOn(nameof(AuthenticateWithToken))]
    [DependsOn(nameof(AuthenticateWithToken_LimitedToTokenPermissions))]
    public async Task DeleteUserAccessToken()
    {
        var admin = await zammadStack.GetClientAsync();
        var client = await GetTokenClientAsync();

        await admin.DeleteUserAccessTokenAsync(CreatedTokenId);

        var list = await admin.ListUserAccessTokensAsync();
        await Assert.That(list.Tokens).DoesNotContain(t => t.Id == CreatedTokenId);

        var exception = await Assert.ThrowsAsync<ZammadException>(() => client.GetUserMeAsync());
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task DeleteUserAccessToken_ThrowsForMissingToken()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.DeleteUserAccessTokenAsync(new UserAccessTokenId(int.MaxValue))
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("The API token could not be found.");
    }
}
