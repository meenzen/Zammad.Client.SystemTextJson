using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client;

internal static class Serialization
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
