using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

public class TagList
{
    [JsonPropertyName("tags")]
    public List<Tag> Tags { get; set; } = new List<Tag>();
}
