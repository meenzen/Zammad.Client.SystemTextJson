using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IOnlineNotificationService
{
    Task<List<OnlineNotification>> ListOnlineNotificationsAsync(Pagination? pagination = null, bool expand = true);

    /// <summary>
    /// Gets an online notification of the current user.
    /// </summary>
    /// <exception cref="ZammadException">
    /// Thrown with <see cref="System.Net.HttpStatusCode.Forbidden"/> if the notification belongs to another user or the
    /// current user can no longer access its ticket. Since Zammad 7.2.1 also if the notification doesn't exist. Older
    /// versions return <c>null</c> in that case.
    /// </exception>
    Task<OnlineNotification?> GetOnlineNotificationAsync(NotificationId id);
    Task<OnlineNotification> UpdateOnlineNotificationAsync(NotificationId id, OnlineNotification notification);
    Task DeleteOnlineNotificationAsync(NotificationId id);
    Task MarkAllNotificationsAsReadAsync();

    /// <summary>
    /// Deletes all online notifications of the current user.
    /// </summary>
    Task DeleteAllOnlineNotificationsAsync();
}

public sealed partial class ZammadClient : IOnlineNotificationService
{
    private const string OnlineNotificationsEndpoint = "/api/v1/online_notifications";

    public async Task<List<OnlineNotification>> ListOnlineNotificationsAsync(
        Pagination? pagination = null,
        bool expand = true
    )
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        builder.Add("expand", expand);
        return await GetAsync<List<OnlineNotification>>(OnlineNotificationsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<OnlineNotification?> GetOnlineNotificationAsync(NotificationId id) =>
        await GetAsync<OnlineNotification>($"{OnlineNotificationsEndpoint}/{id}");

    public async Task<OnlineNotification> UpdateOnlineNotificationAsync(
        NotificationId id,
        OnlineNotification notification
    ) =>
        await PutAsync<OnlineNotification>($"{OnlineNotificationsEndpoint}/{id}", notification)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteOnlineNotificationAsync(NotificationId id) =>
        await DeleteAsync<object>($"{OnlineNotificationsEndpoint}/{id}");

    public async Task MarkAllNotificationsAsReadAsync() =>
        await PostAsync<object>($"{OnlineNotificationsEndpoint}/mark_all_as_read");

    public async Task DeleteAllOnlineNotificationsAsync() =>
        await DeleteAsync<object>($"{OnlineNotificationsEndpoint}/clear_all");
}
