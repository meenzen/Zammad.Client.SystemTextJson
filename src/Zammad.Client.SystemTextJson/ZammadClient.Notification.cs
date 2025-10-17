using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IOnlineNotificationService
{
    Task<List<OnlineNotification>> ListOnlineNotificationsAsync();
    Task<List<OnlineNotification>> ListOnlineNotificationsAsync(int page, int count);
    Task<OnlineNotification?> GetOnlineNotificationAsync(NotificationId id);
    Task<OnlineNotification> CreateOnlineNotificationAsync(OnlineNotification notification);
    Task<OnlineNotification> UpdateOnlineNotificationAsync(NotificationId id, OnlineNotification notification);
    Task<bool> DeleteOnlineNotificationAsync(NotificationId id);
    Task<bool> MarkAllAsReadAsync();
}

public sealed partial class ZammadClient : IOnlineNotificationService
{
    public async Task<List<OnlineNotification>> ListOnlineNotificationsAsync() =>
        await GetAsync<List<OnlineNotification>>("/api/v1/online_notifications") ?? [];

    public async Task<List<OnlineNotification>> ListOnlineNotificationsAsync(int page, int count)
    {
        var builder = new QueryBuilder();
        builder.Add("page", page);
        builder.Add("per_page", count);
        return await GetAsync<List<OnlineNotification>>("/api/v1/online_notifications", builder.ToString()) ?? [];
    }

    public async Task<OnlineNotification?> GetOnlineNotificationAsync(NotificationId id) =>
        await GetAsync<OnlineNotification>($"/api/v1/online_notifications/{id}");

    public async Task<OnlineNotification> CreateOnlineNotificationAsync(OnlineNotification notification) =>
        await PostAsync<OnlineNotification>("/api/v1/online_notifications", notification)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<OnlineNotification> UpdateOnlineNotificationAsync(
        NotificationId id,
        OnlineNotification notification
    ) =>
        await PutAsync<OnlineNotification>($"/api/v1/online_notifications/{id}", notification)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteOnlineNotificationAsync(NotificationId id) =>
        await DeleteAsync<bool>($"/api/v1/online_notifications/{id}");

    public async Task<bool> MarkAllAsReadAsync() =>
        await PostAsync<bool>("/api/v1/online_notifications/mark_all_as_read");
}
