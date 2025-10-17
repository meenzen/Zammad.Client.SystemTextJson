using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IOnlineNotificationService
{
    Task<List<OnlineNotification>> ListOnlineNotificationsAsync();

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<OnlineNotification>> ListOnlineNotificationsAsync(int page, int count);

    Task<List<OnlineNotification>> ListOnlineNotificationsAsync(Pagination? pagination);
    Task<OnlineNotification?> GetOnlineNotificationAsync(NotificationId id);
    Task<OnlineNotification> CreateOnlineNotificationAsync(OnlineNotification notification);
    Task<OnlineNotification> UpdateOnlineNotificationAsync(NotificationId id, OnlineNotification notification);
    Task<bool> DeleteOnlineNotificationAsync(NotificationId id);
    Task<bool> MarkAllAsReadAsync();
}

public sealed partial class ZammadClient : IOnlineNotificationService
{
    private const string OnlineNotificationsEndpoint = "/api/v1/online_notifications";

    public async Task<List<OnlineNotification>> ListOnlineNotificationsAsync() =>
        await GetAsync<List<OnlineNotification>>(OnlineNotificationsEndpoint) ?? [];

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<OnlineNotification>> ListOnlineNotificationsAsync(int page, int count) =>
        await ListOnlineNotificationsAsync(new Pagination { Page = page, PerPage = count });

    public async Task<List<OnlineNotification>> ListOnlineNotificationsAsync(Pagination? pagination)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<OnlineNotification>>(OnlineNotificationsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<OnlineNotification?> GetOnlineNotificationAsync(NotificationId id) =>
        await GetAsync<OnlineNotification>($"{OnlineNotificationsEndpoint}/{id}");

    public async Task<OnlineNotification> CreateOnlineNotificationAsync(OnlineNotification notification) =>
        await PostAsync<OnlineNotification>(OnlineNotificationsEndpoint, notification)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<OnlineNotification> UpdateOnlineNotificationAsync(
        NotificationId id,
        OnlineNotification notification
    ) =>
        await PutAsync<OnlineNotification>($"{OnlineNotificationsEndpoint}/{id}", notification)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteOnlineNotificationAsync(NotificationId id) =>
        await DeleteAsync<bool>($"{OnlineNotificationsEndpoint}/{id}");

    public async Task<bool> MarkAllAsReadAsync() =>
        await PostAsync<bool>($"{OnlineNotificationsEndpoint}/mark_all_as_read");
}
