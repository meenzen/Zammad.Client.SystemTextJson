using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IOnlineNotificationService
{
    Task<List<OnlineNotification>> ListOnlineNotificationsAsync(Pagination? pagination = null, bool expand = true);
    Task<OnlineNotification?> GetOnlineNotificationAsync(NotificationId id);
    Task<OnlineNotification> UpdateOnlineNotificationAsync(NotificationId id, OnlineNotification notification);
    Task DeleteOnlineNotificationAsync(NotificationId id);
    Task MarkAllNotificationsAsReadAsync();
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
}
