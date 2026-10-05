using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class TwoFactorMethod
{
    /// <summary>
    /// The method name, e.g. <c>authenticator_app</c> or <c>security_keys</c>.
    /// </summary>
    [JsonPropertyName("method")]
    public required string Method { get; set; }

    [JsonPropertyName("configured")]
    public bool Configured { get; set; }

    [JsonPropertyName("default")]
    public bool Default { get; set; }
}
