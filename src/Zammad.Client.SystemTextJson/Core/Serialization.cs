using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Core;

internal static class Serialization
{
    internal static readonly JsonSerializerOptions Options = GetOptions();

    internal static JsonSerializerOptions GetOptions() =>
        new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNameCaseInsensitive = true };
}
