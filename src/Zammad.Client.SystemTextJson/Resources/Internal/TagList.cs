using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal class TagList
{
    [JsonPropertyName("tags")]
    internal List<Tag> Tags { get; set; } = [];
}
