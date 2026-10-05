using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class OnlineNotificationTests(ZammadStackFixture zammadStack)
{
    private const string AgentLogin = "agent1@example.org";
    private static readonly TimeSpan NotificationTimeout = TimeSpan.FromSeconds(60);

    private static NotificationId NotificationId { get; set; } = NotificationId.Empty;
    private static TicketId TicketId { get; set; } = TicketId.Empty;

    [Test]
    public async Task ListOnlineNotifications()
    {
        var client = await zammadStack.GetClientAsync();
        var notification = await client.ListOnlineNotificationsAsync();
        await Assert.That(notification).IsNotNull();
    }

    [Test]
    [DependsOn(nameof(CreateOnlineNotification))]
    public async Task ListOnlineNotifications_NotExpanded()
    {
        var client = await zammadStack.GetClientAsync();

        var notifications = await client.ListOnlineNotificationsAsync(
            new Pagination { Page = 1, PerPage = 100 },
            expand: false
        );

        var notification = notifications.Find(n => n.Id == NotificationId);
        await Assert.That(notification).IsNotNull();
        // The expanded names (object, type, user, ...) are only included with expand=true
        await Assert.That(notification!.ObjectType).IsNull();
        await Assert.That(notification.ObjectLookupId).IsNotNull();
    }

    [Test]
    public async Task CreateOnlineNotification()
    {
        var (ticket, notification) = await CreateTicketAndWaitForNotificationAsync();
        TicketId = ticket.Id;

        await Assert.That(notification).IsNotNull();
        await Assert.That(notification!.Type).IsEqualTo("create");
        NotificationId = notification.Id;
    }

    private async Task<(Ticket Ticket, OnlineNotification? Notification)> CreateTicketAndWaitForNotificationAsync()
    {
        var client = await zammadStack.GetClientAsync();
        var me = await client.GetUserMeAsync();

        // Zammad doesn't notify the user who made a change, so another agent has to create the ticket. Owners get an
        // online notification for new tickets with the default notification settings.
        var agentClient = await zammadStack.GetClientOnBehalfOfAsync(AgentLogin);
        var ticket = await agentClient.CreateTicketAsync(
            new Ticket
            {
                Title = "Notification Test Ticket " + TestSetup.RandomString(),
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
                OwnerId = me.Id,
            },
            new TicketArticle
            {
                Subject = "Notification Test",
                Body = "Test notification",
                Type = "note",
            }
        );
        await Assert.That(ticket).IsNotNull();

        // Notifications are created asynchronously by a background job in the scheduler.
        var timeout = DateTimeOffset.UtcNow + NotificationTimeout;
        OnlineNotification? notification = null;
        while (notification is null && DateTimeOffset.UtcNow < timeout)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var notifications = await client.ListOnlineNotificationsAsync(new Pagination { Page = 1, PerPage = 100 });
            notification = notifications.Find(n =>
                n.ObjectType == ObjectType.Ticket && n.ObjectId == ticket.Id.ToTargetObjectId()
            );
        }

        return (ticket, notification);
    }

    [Test]
    [DependsOn(nameof(CreateOnlineNotification))]
    public async Task GetOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();

        var notification = await client.GetOnlineNotificationAsync(NotificationId);

        await Assert.That(notification).IsNotNull();
        await Assert.That(notification!.Id).IsEqualTo(NotificationId);
        await Assert.That(notification.ObjectId).IsEqualTo(TicketId.ToTargetObjectId());
    }

    [Test]
    [DependsOn(nameof(GetOnlineNotification))]
    [DependsOn(nameof(ListOnlineNotifications_NotExpanded))]
    public async Task UpdateOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();

        var notification = await client.GetOnlineNotificationAsync(NotificationId);
        await Assert.That(notification).IsNotNull();
        notification!.Seen = true;

        var updated = await client.UpdateOnlineNotificationAsync(NotificationId, notification);

        await Assert.That(updated).IsNotNull();
        await Assert.That(updated.Seen).IsTrue();

        // Mark it unseen again, so MarkAllAsRead has something to do
        notification.Seen = false;
        updated = await client.UpdateOnlineNotificationAsync(NotificationId, notification);
        await Assert.That(updated.Seen).IsFalse();
    }

    [Test]
    [DependsOn(nameof(UpdateOnlineNotification))]
    public async Task MarkAllAsRead()
    {
        var client = await zammadStack.GetClientAsync();
        await client.MarkAllNotificationsAsReadAsync();

        var notification = await client.GetOnlineNotificationAsync(NotificationId);
        await Assert.That(notification).IsNotNull();
        await Assert.That(notification!.Seen).IsTrue();
    }

    [Test]
    [DependsOn(nameof(MarkAllAsRead))]
    public async Task DeleteOnlineNotification()
    {
        var client = await zammadStack.GetClientAsync();
        await client.DeleteOnlineNotificationAsync(NotificationId);

        await Assert.That(await client.GetOnlineNotificationAsync(NotificationId)).IsNull();
    }

    [Test]
    // Deletes all notifications of the admin, so it has to run after the other tests of this class
    [DependsOn(nameof(DeleteOnlineNotification))]
    public async Task DeleteAllOnlineNotifications()
    {
        var (_, notification) = await CreateTicketAndWaitForNotificationAsync();
        await Assert.That(notification).IsNotNull();

        var client = await zammadStack.GetClientAsync();
        await client.DeleteAllOnlineNotificationsAsync();

        await Assert.That(await client.GetOnlineNotificationAsync(notification!.Id)).IsNull();
    }
}
