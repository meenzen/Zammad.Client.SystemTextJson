using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class OnlineNotificationTests(ZammadStackFixture zammadStack)
{
    private static NotificationId CreatedNotificationId { get; set; } = NotificationId.Empty;

    [Test]
    public async Task CreateOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();

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
            }
        );

        await Assert.That(ticket).IsNotNull();

        var notification = await client.CreateOnlineNotificationAsync(
            new OnlineNotification
            {
                ObjectId = new ObjectId(ticket.Id.Value),
                Object = "Ticket",
                Type = "mention",
                Seen = false,
            }
        );

        await Assert.That(notification).IsNotNull();
        await Assert.That(notification.Id).IsNotEqualTo(NotificationId.Empty);

        CreatedNotificationId = notification.Id;
    }

    [Test]
    [DependsOn(nameof(CreateOnlineNotification))]
    public async Task ListOnlineNotifications()
    {
        var client = await zammadStack.GetClientAsync();

        var notifications = await client.ListOnlineNotificationsAsync();

        await Assert.That(notifications).IsNotNull();
        await Assert.That(notifications).Contains(n => n.Id == CreatedNotificationId);
    }

    [Test]
    [DependsOn(nameof(CreateOnlineNotification))]
    public async Task ListOnlineNotifications_Pagination()
    {
        var client = await zammadStack.GetClientAsync();

        var notifications = await client.ListOnlineNotificationsAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(notifications).IsNotNull();
        await Assert.That(notifications).Contains(n => n.Id == CreatedNotificationId);
    }

    [Test]
    [DependsOn(nameof(CreateOnlineNotification))]
    [Obsolete("Testing legacy pagination.")]
    public async Task ListOnlineNotifications_Pagination_Legacy()
    {
        var client = await zammadStack.GetClientAsync();

        var notifications = await client.ListOnlineNotificationsAsync(1, 100);

        await Assert.That(notifications).IsNotNull();
        await Assert.That(notifications).Contains(n => n.Id == CreatedNotificationId);
    }

    [Test]
    [DependsOn(nameof(ListOnlineNotifications))]
    [DependsOn(nameof(ListOnlineNotifications_Pagination))]
    [DependsOn(nameof(ListOnlineNotifications_Pagination_Legacy))]
    public async Task GetOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();

        var notification = await client.GetOnlineNotificationAsync(CreatedNotificationId);

        await Assert.That(notification).IsNotNull();
        await Assert.That(notification!.Id).IsEqualTo(CreatedNotificationId);
        await Assert.That(notification.Object).IsEqualTo("Ticket");
        await Assert.That(notification.Type).IsEqualTo("mention");
    }

    [Test]
    [DependsOn(nameof(GetOnlineNotification))]
    public async Task UpdateOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();

        var notification = await client.GetOnlineNotificationAsync(CreatedNotificationId);
        await Assert.That(notification).IsNotNull();
        notification!.Seen = true;

        var updated = await client.UpdateOnlineNotificationAsync(CreatedNotificationId, notification);

        await Assert.That(updated).IsNotNull();
        await Assert.That(updated.Seen).IsTrue();
    }

    [Test]
    [DependsOn(nameof(UpdateOnlineNotification))]
    public async Task MarkAllAsRead()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.MarkAllAsReadAsync();

        await Assert.That(result).IsTrue();
    }

    [Test]
    [DependsOn(nameof(MarkAllAsRead))]
    public async Task DeleteOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.DeleteOnlineNotificationAsync(CreatedNotificationId);

        await Assert.That(result).IsTrue();
    }
}
