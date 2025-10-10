using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Abstractions;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public sealed partial class ZammadClient : IOnlineNotificationService
{
    public Task<List<OnlineNotification>> GetOnlineNotificationListAsync() =>
        GetAsync<List<OnlineNotification>>("/api/v1/online_notifications");

    public Task<List<OnlineNotification>> GetOnlineNotificationListAsync(int page, int count) =>
        GetAsync<List<OnlineNotification>>("/api/v1/online_notifications", $"page={page}&per_page={count}");

    public Task<OnlineNotification> GetOnlineNotificationAsync(int id) =>
        GetAsync<OnlineNotification>($"/api/v1/online_notifications/{id}");

    public Task<OnlineNotification> CreateOnlineNotificationAsync(OnlineNotification notification) =>
        PostAsync<OnlineNotification>("/api/v1/online_notifications", notification);

    public Task<OnlineNotification> UpdateOnlineNotificationAsync(int id, OnlineNotification notification) =>
        PutAsync<OnlineNotification>($"/api/v1/online_notifications/{id}", notification);

    public Task<bool> DeleteOnlineNotificationAsync(int id) => DeleteAsync<bool>($"/api/v1/online_notifications/{id}");

    public Task<bool> MarkAllAsReadAsync() => PostAsync<bool>("/api/v1/online_notifications/mark_all_as_read");
}
