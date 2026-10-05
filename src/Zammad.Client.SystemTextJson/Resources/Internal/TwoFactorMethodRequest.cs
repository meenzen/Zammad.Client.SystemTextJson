using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class TwoFactorMethodRequest
{
    [JsonPropertyName("method")]
    public required string Method { get; set; }
}
