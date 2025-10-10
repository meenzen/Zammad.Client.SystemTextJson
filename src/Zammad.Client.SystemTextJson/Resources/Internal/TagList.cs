using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

public class TagList
{
    [JsonPropertyName("tags")]
    public IList<Tag> Tags { get; set; } = new List<Tag>();
}
