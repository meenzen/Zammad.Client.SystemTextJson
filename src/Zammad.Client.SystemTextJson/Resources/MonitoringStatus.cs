using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class MonitoringStatus
{
    /// <summary>
    /// Number of records per table, e.g. <c>users</c>, <c>tickets</c> or <c>ticket_articles</c>.
    /// </summary>
    [JsonPropertyName("counts")]
    public Dictionary<string, long> Counts { get; set; } = [];

    /// <summary>
    /// When the last record of each table in <see cref="Counts"/> was created, or <c>null</c> if the table is empty.
    /// </summary>
    [JsonPropertyName("last_created_at")]
    public Dictionary<string, DateTimeOffset?> LastCreatedAt { get; set; } = [];

    [JsonPropertyName("last_login")]
    public DateTimeOffset? LastLogin { get; set; }

    /// <summary>
    /// Number of users with the <c>ticket.agent</c> permission.
    /// </summary>
    [JsonPropertyName("agents")]
    public long Agents { get; set; }

    /// <summary>
    /// Total size of all stored files, or <c>null</c> if there are none.
    /// </summary>
    [JsonPropertyName("storage")]
    public MonitoringStorage? Storage { get; set; }
}

public sealed class MonitoringStorage
{
    [JsonPropertyName("kB")]
    public long Kilobytes { get; set; }

    [JsonPropertyName("MB")]
    public long Megabytes { get; set; }

    [JsonPropertyName("GB")]
    public long Gigabytes { get; set; }
}
