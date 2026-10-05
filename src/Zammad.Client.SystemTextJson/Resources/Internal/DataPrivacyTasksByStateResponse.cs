using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class DataPrivacyTasksByStateResponse
{
    [JsonPropertyName("record_ids")]
    public DataPrivacyTaskRecordIds? RecordIds { get; set; }

    /// <summary>
    /// The tasks and the users that created or updated them, by model name and ID.
    /// </summary>
    [JsonPropertyName("assets")]
    public Dictionary<string, Dictionary<string, JsonElement>>? Assets { get; set; }
}

internal sealed class DataPrivacyTaskRecordIds
{
    [JsonPropertyName("in_process")]
    public List<DataPrivacyTaskId>? InProcess { get; set; }

    [JsonPropertyName("failed")]
    public List<DataPrivacyTaskId>? Failed { get; set; }

    [JsonPropertyName("completed")]
    public List<DataPrivacyTaskId>? Completed { get; set; }
}
