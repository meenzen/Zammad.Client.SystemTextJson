using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class SettingValueRequest<T>(T value)
{
    [JsonPropertyName("state_current")]
    public SettingState<T> StateCurrent { get; } = new(value);
}
