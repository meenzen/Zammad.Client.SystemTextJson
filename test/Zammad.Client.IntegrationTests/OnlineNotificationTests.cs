using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class OnlineNotificationTests(ZammadStackFixture zammadStack)
{
    private static NotificationId NotificationId { get; set; } = NotificationId.Empty;

    [Test]
    public async Task ListOnlineNotifications()
    {
        var client = await zammadStack.GetClientAsync();
        var notification = await client.ListOnlineNotificationsAsync();
        await Assert.That(notification).IsNotNull();
    }

    [Test]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    [Skip("Currently does not work, find out how to reliably trigger notifications in tests.")]
    public async Task CreateOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();

        var user = await client.GetUserMeAsync();
        var ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = "Notification Test Ticket",
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
                OwnerId = new UserId(1),
            },
            new TicketArticle
            {
                Subject = "Notification Test",
                Body = "Test notification",
                Type = "note",
                CC = user.Email,
            }
        );

        await Assert.That(ticket).IsNotNull();

        await Task.Delay(TestSetup.IndexerDelay);

        var notifications = await client.ListOnlineNotificationsAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(notifications).IsNotEmpty();
        NotificationId = notifications.First().Id;
    }

    [Test]
    [DependsOn(nameof(CreateOnlineNotification))]
    public async Task GetOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();

        var notification = await client.GetOnlineNotificationAsync(NotificationId);

        await Assert.That(notification).IsNotNull();
        await Assert.That(notification!.Id).IsEqualTo(NotificationId);
        await Assert.That(notification.ObjectType).IsEqualTo(ObjectType.Ticket);
        await Assert.That(notification.Type).IsEqualTo("mention");
    }

    [Test]
    [DependsOn(nameof(GetOnlineNotification))]
    public async Task UpdateOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();

        var notification = await client.GetOnlineNotificationAsync(NotificationId);
        await Assert.That(notification).IsNotNull();
        notification!.Seen = true;

        var updated = await client.UpdateOnlineNotificationAsync(NotificationId, notification);

        await Assert.That(updated).IsNotNull();
        await Assert.That(updated.Seen).IsTrue();
    }

    [Test]
    [DependsOn(nameof(UpdateOnlineNotification))]
    public async Task MarkAllAsRead()
    {
        var client = await zammadStack.GetClientAsync();
        await client.MarkAllNotificationsAsReadAsync();
    }

    [Test]
    [DependsOn(nameof(MarkAllAsRead))]
    public async Task DeleteOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();
        await client.DeleteOnlineNotificationAsync(NotificationId);
    }
}
