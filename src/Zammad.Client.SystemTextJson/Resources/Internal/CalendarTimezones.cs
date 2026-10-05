using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class CalendarTimezones
{
    [JsonPropertyName("timezones")]
    public Dictionary<string, int>? Timezones { get; set; }
}
