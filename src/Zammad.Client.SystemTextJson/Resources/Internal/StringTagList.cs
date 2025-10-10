using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

#nullable enable

internal class StringTagList
{
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];
}
