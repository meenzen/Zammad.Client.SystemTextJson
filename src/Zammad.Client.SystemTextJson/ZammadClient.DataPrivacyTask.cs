using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

/// <summary>
/// Data privacy tasks delete a user together with their tickets (and optionally their organization), or a ticket, in
/// the background. Requires the <c>admin.data_privacy</c> permission.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="IUserService.DeleteUserAsync"/>, which fails with 422 "Can't delete, object has references." as
/// long as anything refers to the user, a task deletes the tickets of the customer and moves the remaining references
/// (owner, created by, updated by, ...) to the system user (ID 1). The user is set inactive first.
/// </para>
/// <para>
/// Tasks are asynchronous and irreversible. The scheduler runs the pending tasks every 10 minutes, so it can take
/// that long (plus the deletion itself) until a task is <see cref="DataPrivacyTaskState.Completed"/>. Poll
/// <see cref="GetDataPrivacyTaskAsync"/> to wait for it. Zammad deletes tasks older than 12 months.
/// </para>
/// </remarks>
public interface IDataPrivacyTaskService
{
    Task<List<DataPrivacyTask>> ListDataPrivacyTasksAsync(Pagination? pagination = null);

    /// <summary>
    /// Returns the 500 most recent tasks, grouped by state.
    /// </summary>
    Task<DataPrivacyTasksByState> ListDataPrivacyTasksByStateAsync();

    Task<DataPrivacyTask?> GetDataPrivacyTaskAsync(DataPrivacyTaskId id);

    /// <summary>
    /// Creates a task that deletes a user, the tickets of the user as customer, and optionally the organization.
    /// </summary>
    /// <param name="userId">The user. It can't be the system user (ID 1), the current user, or the last admin.</param>
    /// <param name="deleteOrganization">
    /// Also delete the organization of the user (with its tickets), if the user is its only member. Otherwise only the
    /// user is deleted.
    /// </param>
    /// <exception cref="ZammadException">
    /// 422 if the user can't be deleted, or a task for the user is already in process.
    /// </exception>
    Task<DataPrivacyTask> CreateUserDataPrivacyTaskAsync(UserId userId, bool deleteOrganization = false);

    /// <summary>
    /// Creates a task that deletes a ticket.
    /// </summary>
    /// <exception cref="ZammadException">422 if a task for the ticket is already in process.</exception>
    Task<DataPrivacyTask> CreateTicketDataPrivacyTaskAsync(TicketId ticketId);

    /// <summary>
    /// Creates a task from <see cref="DataPrivacyTask.DeletableType"/>, <see cref="DataPrivacyTask.DeletableId"/> and
    /// <see cref="DataPrivacyTask.Preferences"/>. Only <c>User</c> and <c>Ticket</c> are supported. To delete an
    /// organization, delete its only member with <c>delete_organization</c>.
    /// </summary>
    Task<DataPrivacyTask> CreateDataPrivacyTaskAsync(DataPrivacyTask task);

    /// <summary>
    /// Updates a task, e.g. sets the <see cref="DataPrivacyTask.State"/> of a failed task back to
    /// <see cref="DataPrivacyTaskState.InProcess"/> to retry it.
    /// </summary>
    Task<DataPrivacyTask> UpdateDataPrivacyTaskAsync(DataPrivacyTaskId id, DataPrivacyTask task);

    /// <summary>
    /// Deletes the task. If the scheduler hasn't run it yet, nothing is deleted.
    /// </summary>
    Task DeleteDataPrivacyTaskAsync(DataPrivacyTaskId id);
}

public sealed partial class ZammadClient : IDataPrivacyTaskService
{
    private const string DataPrivacyTasksEndpoint = "/api/v1/data_privacy_tasks";

    public async Task<List<DataPrivacyTask>> ListDataPrivacyTasksAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<DataPrivacyTask>>(DataPrivacyTasksEndpoint, builder.ToString()) ?? [];
    }

    public async Task<DataPrivacyTasksByState> ListDataPrivacyTasksByStateAsync()
    {
        var response =
            await GetAsync<DataPrivacyTasksByStateResponse>($"{DataPrivacyTasksEndpoint}/by_state")
            ?? throw LogicException.UnexpectedNullResult;

        Dictionary<string, JsonElement>? assets = null;
        response.Assets?.TryGetValue("DataPrivacyTask", out assets);

        List<DataPrivacyTask> Resolve(List<DataPrivacyTaskId>? ids) =>
            (ids ?? [])
                .Select(id =>
                    assets is not null && assets.TryGetValue(id.ToString(), out var asset)
                        ? asset.Deserialize<DataPrivacyTask>(Serialization.Options)
                        : null
                )
                .OfType<DataPrivacyTask>()
                .ToList();

        return new DataPrivacyTasksByState
        {
            InProcess = Resolve(response.RecordIds?.InProcess),
            Failed = Resolve(response.RecordIds?.Failed),
            Completed = Resolve(response.RecordIds?.Completed),
        };
    }

    public async Task<DataPrivacyTask?> GetDataPrivacyTaskAsync(DataPrivacyTaskId id) =>
        await GetAsync<DataPrivacyTask>($"{DataPrivacyTasksEndpoint}/{id}");

    public async Task<DataPrivacyTask> CreateUserDataPrivacyTaskAsync(UserId userId, bool deleteOrganization = false) =>
        await PostAsync<DataPrivacyTask>(
            DataPrivacyTasksEndpoint,
            new DataPrivacyTaskRequest
            {
                DeletableType = DataPrivacyTaskDeletableType.User,
                DeletableId = userId.ToTargetObjectId(),
                // Zammad compares with the string "true"
                Preferences = new Dictionary<string, string>
                {
                    ["delete_organization"] = deleteOrganization ? "true" : "false",
                },
            }
        ) ?? throw LogicException.UnexpectedNullResult;

    public async Task<DataPrivacyTask> CreateTicketDataPrivacyTaskAsync(TicketId ticketId) =>
        await PostAsync<DataPrivacyTask>(
            DataPrivacyTasksEndpoint,
            new DataPrivacyTaskRequest
            {
                DeletableType = DataPrivacyTaskDeletableType.Ticket,
                DeletableId = ticketId.ToTargetObjectId(),
            }
        ) ?? throw LogicException.UnexpectedNullResult;

    public async Task<DataPrivacyTask> CreateDataPrivacyTaskAsync(DataPrivacyTask task) =>
        await PostAsync<DataPrivacyTask>(DataPrivacyTasksEndpoint, task) ?? throw LogicException.UnexpectedNullResult;

    public async Task<DataPrivacyTask> UpdateDataPrivacyTaskAsync(DataPrivacyTaskId id, DataPrivacyTask task) =>
        await PutAsync<DataPrivacyTask>($"{DataPrivacyTasksEndpoint}/{id}", task)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteDataPrivacyTaskAsync(DataPrivacyTaskId id) =>
        await DeleteAsync<object>($"{DataPrivacyTasksEndpoint}/{id}");
}
