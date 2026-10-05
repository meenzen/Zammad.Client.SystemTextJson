using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class OverviewPrioRequest
{
    /// <summary>
    /// <c>[[overview_id, prio], ...]</c>
    /// </summary>
    [JsonPropertyName("prios")]
    public required List<int[]> Prios { get; set; }

    internal static OverviewPrioRequest Create(IReadOnlyDictionary<OverviewId, int> priorities) =>
        new() { Prios = priorities.Select(p => new[] { p.Key.Value, p.Value }).ToList() };
}
