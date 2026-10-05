using System.Net;
using Microsoft.Extensions.Options;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class ErrorHandlingTests(ZammadStackFixture zammadStack)
{
    private const int MissingId = int.MaxValue;

    [Test]
    public async Task Get_ReturnsNullForMissingRecord()
    {
        var client = await zammadStack.GetClientAsync();

        await Assert.That(await client.GetUserAsync(new UserId(MissingId))).IsNull();
        await Assert.That(await client.GetOrganizationAsync(new OrganizationId(MissingId))).IsNull();
        await Assert.That(await client.GetGroupAsync(new GroupId(MissingId))).IsNull();
        await Assert.That(await client.GetTicketAsync(new TicketId(MissingId))).IsNull();
        await Assert.That(await client.GetTicketArticleAsync(new ArticleId(MissingId))).IsNull();
        await Assert.That(await client.GetTicketStateAsync(new StateId(MissingId))).IsNull();
        await Assert.That(await client.GetTicketPriorityAsync(new PriorityId(MissingId))).IsNull();
        await Assert.That(await client.GetEmailAddressAsync(new EmailAddressId(MissingId))).IsNull();
        await Assert.That(await client.GetObjectAsync(new ObjectId(MissingId))).IsNull();
        await Assert.That(await client.GetOnlineNotificationAsync(new NotificationId(MissingId))).IsNull();
    }

    [Test]
    public async Task Delete_ThrowsForMissingRecord()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.DeleteGroupAsync(new GroupId(MissingId))
        );

        // Not a 404: Zammad's reference check before deleting turns every error into a 422
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).StartsWith("Couldn't find Group");
    }

    [Test]
    public async Task Create_ThrowsWithZammadErrorForInvalidRecord()
    {
        var client = await zammadStack.GetClientAsync();

        // "2 normal" is one of Zammad's default priorities, and priority names are unique
        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateTicketPriorityAsync(new TicketPriority { Name = "2 normal", Active = true })
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        // Zammad only renders JSON errors (instead of an HTML page) when the client asks for JSON
        await Assert.That(exception.Content).StartsWith("{");
        await Assert.That(exception.Error).IsNotNullOrWhiteSpace();
        await Assert.That(exception.Message).EndsWith(": " + exception.Error);
    }

    [Test]
    public async Task Request_ThrowsForInvalidCredentials()
    {
        var client = new ZammadClient(
            new HttpClient(),
            Options.Create(
                new ZammadOptions
                {
                    BaseUrl = await zammadStack.GetPublicUriAsync(),
                    Username = "admin@example.org",
                    Password = "wrong password",
                }
            )
        );

        var exception = await Assert.ThrowsAsync<ZammadException>(() => client.GetUserMeAsync());

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(exception.Error).IsNotNullOrWhiteSpace();
    }
}
