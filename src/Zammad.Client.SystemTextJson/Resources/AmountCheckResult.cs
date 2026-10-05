using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class AmountCheckResult
{
    /// <summary>
    /// <c>ok</c>, <c>warning</c> or <c>critical</c>, or <c>null</c> if no thresholds were given.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// Describes the threshold that was exceeded or undercut. <c>null</c> if the state is <c>ok</c>.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Number of tickets created within the period.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }
}
