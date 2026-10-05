using System.Text.Json;
using System.Text.Json.Serialization;
using Zammad.Client.Core;

namespace Zammad.Client.Resources;

/// <summary>
/// A background task that deletes a user (with their tickets, and optionally their organization) or a ticket.
/// </summary>
/// <remarks>
/// The scheduler runs the pending tasks every 10 minutes. The deletion is irreversible.
/// </remarks>
public sealed class DataPrivacyTask
{
    [JsonPropertyName("id")]
    public DataPrivacyTaskId Id { get; set; }

    /// <summary>
    /// <c>in process</c>, <c>completed</c> or <c>failed</c>, see <see cref="DataPrivacyTaskState"/>.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// <c>User</c> or <c>Ticket</c>, see <see cref="DataPrivacyTaskDeletableType"/>.
    /// </summary>
    [JsonPropertyName("deletable_type")]
    public string? DeletableType { get; set; }

    /// <summary>
    /// The ID of the user or ticket, depending on <see cref="DeletableType"/>.
    /// </summary>
    [JsonPropertyName("deletable_id")]
    public TargetObjectId? DeletableId { get; set; }

    /// <summary>
    /// The options of the task and the preview of what it deletes. Zammad computes the preview when the task is
    /// created and again when it runs. See the typed accessors, e.g. <see cref="CustomerTicketsCount"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>delete_organization</c>: <c>"true"</c> (a string, a JSON <c>true</c> is ignored) to also delete the
    /// organization of the user, if the user is its only member.</item>
    /// <item><c>owner_tickets</c>/<c>owner_tickets_count</c>: up to 1000 numbers of the tickets the user owns. They
    /// aren't deleted.</item>
    /// <item><c>customer_tickets</c>/<c>customer_tickets_count</c>: up to 1000 numbers of the tickets that are
    /// deleted, i.e. the tickets of the customer, or the ticket itself.</item>
    /// <item><c>user</c> (<c>firstname</c>, <c>lastname</c>, <c>email</c>, <c>organization</c>) or <c>ticket</c>
    /// (<c>title</c>): the pseudonymized record, e.g. <c>H***r S***n</c>.</item>
    /// <item><c>error</c>: the error, if the task failed.</item>
    /// </list>
    /// </remarks>
    [JsonPropertyName("preferences")]
    public Dictionary<string, JsonElement>? Preferences { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// The numbers of up to 1000 tickets that are deleted: the tickets of the customer, or the ticket itself.
    /// </summary>
    [JsonIgnore]
    public List<string>? CustomerTickets => GetPreference<List<string>>("customer_tickets");

    /// <summary>
    /// The number of tickets that are deleted.
    /// </summary>
    [JsonIgnore]
    public int? CustomerTicketsCount => GetPreference<int?>("customer_tickets_count");

    /// <summary>
    /// The numbers of up to 1000 tickets the user owns. They aren't deleted, their owner is unset.
    /// </summary>
    [JsonIgnore]
    public List<string>? OwnerTickets => GetPreference<List<string>>("owner_tickets");

    /// <summary>
    /// The number of tickets the user owns.
    /// </summary>
    [JsonIgnore]
    public int? OwnerTicketsCount => GetPreference<int?>("owner_tickets_count");

    /// <summary>
    /// The error, if the task <see cref="DataPrivacyTaskState.Failed">failed</see>.
    /// </summary>
    [JsonIgnore]
    public string? Error => GetPreference<string>("error");

    private T? GetPreference<T>(string name) =>
        Preferences is { } preferences && preferences.TryGetValue(name, out var value)
            ? value.Deserialize<T>(Serialization.Options)
            : default;
}

public static class DataPrivacyTaskState
{
    /// <summary>
    /// The task waits for the scheduler.
    /// </summary>
    public const string InProcess = "in process";

    public const string Completed = "completed";

    /// <summary>
    /// The task failed, see <see cref="DataPrivacyTask.Error"/>. Set the state back to <see cref="InProcess"/> to
    /// retry it.
    /// </summary>
    public const string Failed = "failed";
}

public static class DataPrivacyTaskDeletableType
{
    public const string User = "User";
    public const string Ticket = "Ticket";
}
