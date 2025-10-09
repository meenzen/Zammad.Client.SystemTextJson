using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

public class StringTagList
{
    [JsonPropertyName("tags")]
    public IList<string> Tags { get; set; } = new List<string>();
}
